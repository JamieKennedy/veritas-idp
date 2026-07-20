import { execFileSync } from 'node:child_process'
import { createHash } from 'node:crypto'
import { cpSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import path from 'node:path'

const [version] = process.argv.slice(2)
if (!version) {
    throw new Error('The release version is required.')
}

const repository = process.env.GITHUB_REPOSITORY?.toLowerCase()
const revision = process.env.GITHUB_SHA
if (!repository || !revision) {
    throw new Error('GITHUB_REPOSITORY and GITHUB_SHA are required.')
}

const registry = process.env.CONTAINER_REGISTRY ?? 'ghcr.io'
const source = `https://github.com/${process.env.GITHUB_REPOSITORY}`
const shortRevision = revision.slice(0, 12)
const isPrerelease = version.includes('-')
const movingTag = isPrerelease ? 'beta' : 'latest'
const platforms = process.env.RELEASE_PLATFORMS ?? 'linux/amd64,linux/arm64'

buildImage({
    dockerfile: 'src/backend/Dockerfile',
    image: `${registry}/${repository}-backend`,
})
buildImage({
    dockerfile: 'src/frontend/admin-ui/Dockerfile',
    image: `${registry}/${repository}-admin-ui`,
})
createComposeBundle()

function buildImage({ dockerfile, image }) {
    run('docker', [
        'buildx',
        'build',
        '--file',
        dockerfile,
        '--platform',
        platforms,
        '--build-arg',
        `VERSION=${version}`,
        '--build-arg',
        `REVISION=${revision}`,
        '--build-arg',
        `SOURCE=${source}`,
        '--provenance=mode=max',
        '--sbom=true',
        '--tag',
        `${image}:${version}`,
        '--tag',
        `${image}:sha-${shortRevision}`,
        '--tag',
        `${image}:${movingTag}`,
        '--push',
        '.',
    ])
}

function createComposeBundle() {
    const artifactsDirectory = path.resolve('artifacts')
    const bundleName = `veritas-compose-${version}`
    const bundleDirectory = path.join(artifactsDirectory, bundleName)
    const archivePath = path.join(artifactsDirectory, `${bundleName}.tar.gz`)

    rmSync(bundleDirectory, { recursive: true, force: true })
    mkdirSync(path.join(bundleDirectory, 'secrets'), { recursive: true })
    cpSync('compose.yaml', path.join(bundleDirectory, 'compose.yaml'))
    cpSync('docs/operations/docker-compose.md', path.join(bundleDirectory, 'README.md'))
    cpSync('docs/operations/secrets.md', path.join(bundleDirectory, 'secrets', 'README.md'))

    const environmentExample = readFileSync('.env.example', 'utf8').replace('VERITAS_VERSION=0.0.0-development', `VERITAS_VERSION=${version}`)
    writeFileSync(path.join(bundleDirectory, '.env.example'), environmentExample)

    run('tar', ['--create', '--gzip', '--file', archivePath, '--directory', artifactsDirectory, bundleName])
    const checksum = createHash('sha256').update(readFileSync(archivePath)).digest('hex')
    writeFileSync(`${archivePath}.sha256`, `${checksum}  ${path.basename(archivePath)}\n`)
    rmSync(bundleDirectory, { recursive: true, force: true })
}

function run(command, arguments_) {
    execFileSync(command, arguments_, {
        stdio: 'inherit',
    })
}
