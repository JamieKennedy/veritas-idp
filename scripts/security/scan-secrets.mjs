import { execFileSync } from 'node:child_process'
import path from 'node:path'

const repositoryRoot = path.resolve(import.meta.dirname, '../..')
const gitleaksImage = 'ghcr.io/gitleaks/gitleaks:v8.30.1@sha256:c00b6bd0aeb3071cbcb79009cb16a60dd9e0a7c60e2be9ab65d25e6bc8abbb7f'

execFileSync(
    'docker',
    [
        'run',
        '--rm',
        '--volume',
        `${repositoryRoot}:/repository`,
        '--workdir',
        '/repository',
        gitleaksImage,
        'git',
        '--no-banner',
        '--redact',
        '--verbose',
        '/repository',
    ],
    {
        stdio: 'inherit',
    },
)
