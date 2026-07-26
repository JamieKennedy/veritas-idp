import { execFileSync } from 'node:child_process'

const backendImage = 'veritas-backend:quality'
const adminUiImage = 'veritas-admin-ui:quality'
const hadolintImage = 'hadolint/hadolint:v2.14.0-alpine@sha256:7aba693c1442eb31c0b015c129697cb3b6cb7da589d85c7562f9deb435a6657c'
const trivyImage = 'aquasec/trivy:0.69.3@sha256:bcc376de8d77cfe086a917230e818dc9f8528e3c852f7b1aff648949b6258d1c'

run('docker', ['compose', '--env-file', '.env.example', '-f', 'compose.yaml', '-f', 'compose.local.yaml', 'config', '--quiet'])
for (const dockerfile of ['src/backend/Dockerfile', 'src/frontend/admin-ui/Dockerfile']) {
    run('docker', [
        'run',
        '--rm',
        '--volume',
        `${process.cwd()}:/workspace:ro`,
        '--workdir',
        '/workspace',
        hadolintImage,
        'hadolint',
        '--failure-threshold',
        'warning',
        dockerfile,
    ])
}
run('docker', ['build', '--file', 'src/backend/Dockerfile', '--tag', backendImage, '.'])
run('docker', ['build', '--file', 'src/frontend/admin-ui/Dockerfile', '--tag', adminUiImage, '.'])
run('docker', [
    'run',
    '--rm',
    '--entrypoint',
    '/bin/sh',
    backendImage,
    '-c',
    'test -f /app/admin-api/Veritas.Admin.API.dll && test -f /app/migrator/Veritas.Tooling.DbMigrator.dll',
])

for (const image of [backendImage, adminUiImage]) {
    run('docker', [
        'run',
        '--rm',
        '-v',
        '/var/run/docker.sock:/var/run/docker.sock',
        '-v',
        'veritas-trivy-cache:/root/.cache/trivy',
        trivyImage,
        'image',
        '--exit-code',
        '1',
        '--scanners',
        'vuln',
        '--report',
        'summary',
        '--severity',
        'HIGH,CRITICAL',
        '--skip-version-check',
        image,
    ])
}

function run(command, arguments_) {
    execFileSync(command, arguments_, {
        cwd: process.cwd(),
        stdio: 'inherit',
    })
}
