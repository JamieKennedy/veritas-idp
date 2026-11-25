import { execFileSync } from 'node:child_process'
import { randomBytes } from 'node:crypto'
import { mkdtempSync, rmSync, writeFileSync } from 'node:fs'
import { createServer } from 'node:net'
import { tmpdir } from 'node:os'
import path from 'node:path'

const project = `veritas-test-${randomBytes(8).toString('hex')}`
const pnpmCli = process.env.npm_execpath
if (!pnpmCli) {
    throw new Error('Run this check through pnpm.')
}

const listener = createServer()
await new Promise((resolve, reject) => listener.once('error', reject).listen(0, '127.0.0.1', resolve))
const { port } = listener.address()
await new Promise((resolve) => listener.close(resolve))

const directory = mkdtempSync(path.join(tmpdir(), 'veritas-admin-workflow-'))
const secretFile = path.join(directory, 'bootstrap.txt')
const bootstrapSecret = randomBytes(32).toString('hex')
const useQualityImages = process.env.VERITAS_TEST_USE_QUALITY_IMAGES === '1'
const overrideFile = path.join(directory, 'test.yaml')
const composeArgs = ['compose', '--project-name', project, '--file', 'compose.yaml']
if (!useQualityImages) {
    composeArgs.push('--file', 'compose.local.yaml')
}
composeArgs.push('--file', overrideFile)
const env = {
    ...process.env,
    VERITAS_VERSION: '0.0.0-development',
    VERITAS_BIND_ADDRESS: '127.0.0.1',
    VERITAS_POSTGRES_PASSWORD: randomBytes(32).toString('hex'),
    VERITAS_RABBITMQ_PASSWORD: randomBytes(32).toString('hex'),
    VERITAS_BOOTSTRAP_SECRET_FILE: secretFile,
    VERITAS_TEST_BASE_URL: `https://localhost:${port}`,
    VERITAS_TEST_BOOTSTRAP_SECRET: bootstrapSecret,
}

try {
    writeFileSync(secretFile, bootstrapSecret)
    const images = useQualityImages ? '  migrator:\n    image: veritas-backend:quality\n  admin-api:\n    image: veritas-backend:quality\n' : ''
    const uiImage = useQualityImages ? '    image: veritas-admin-ui:quality\n' : ''
    // Use the same HTTPS boundary that protects administrator cookies in a deployment.
    writeFileSync(
        overrideFile,
        `services:
${images}  admin-ui:
${uiImage}    ports: !reset []
  proxy:
    image: caddy:2.10-alpine@sha256:4c6e91c6ed0e2fa03efd5b44747b625fec79bc9cd06ac5235a779726618e530d
    command: ["caddy", "reverse-proxy", "--from", "https://localhost", "--to", "admin-ui:3000", "--internal-certs", "--disable-redirects"]
    ports: ["127.0.0.1:${port}:443"]
    networks: ["edge"]
    depends_on:
      admin-ui:
        condition: service_healthy
    healthcheck:
      test: ["CMD", "wget", "--quiet", "--no-check-certificate", "--output-document=/dev/null", "https://localhost/healthz"]
      interval: 2s
      timeout: 3s
      retries: 15
`,
    )

    compose(['up', '--wait', '--wait-timeout', '180', useQualityImages ? '--no-build' : '--build'])
    verifyHealth('/health', 'Healthy 200')
    execFileSync(process.execPath, [pnpmCli, '--filter', 'admin-ui', 'exec', 'playwright', 'test'], { env, stdio: 'inherit' })

    // Re-run the actual release migrator against the configured PostgreSQL instance.
    compose(['run', '--rm', '--no-deps', 'migrator'])
    const status = JSON.parse(readAdminApi('/api/v1/setup/status', ['--fail']))
    if (!status.isConfigured || !status.isSmtpSetupDeferred) {
        throw new Error('The migrator rerun did not preserve completed setup.')
    }
    compose(['exec', '-T', 'admin-api', 'sh', '-ec', 'grep -Fq "api/v{version:apiVersion}/setup/status responded 200" /var/log/veritas/admin-api-*.log'])

    // A database outage should make readiness fail while the process remains alive.
    compose(['stop', 'postgres'])
    verifyHealth('/health', 'Unhealthy 503')
    verifyHealth('/alive', 'Healthy 200')
} finally {
    try {
        compose(['down', '--volumes', '--remove-orphans'])
    } finally {
        rmSync(directory, { recursive: true, force: true })
    }
}

function compose(args) {
    execFileSync('docker', [...composeArgs, ...args], { env, stdio: 'inherit' })
}

function verifyHealth(route, expected) {
    const result = readAdminApi(route, ['--write-out', ' %{http_code}'])
    if (result !== expected) {
        throw new Error(`Expected ${expected} from ${route}, received ${result}.`)
    }
}

function readAdminApi(route, curlOptions) {
    return execFileSync(
        'docker',
        [...composeArgs, 'exec', '-T', 'admin-api', 'curl', '--silent', '--show-error', '--max-time', '20', ...curlOptions, `http://127.0.0.1:8080${route}`],
        { env, encoding: 'utf8' },
    )
}
