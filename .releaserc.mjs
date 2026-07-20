export default {
    branches: [
        'main',
        {
            name: 'staging',
            prerelease: 'beta',
        },
    ],
    tagFormat: 'v${version}',
    plugins: [
        [
            '@semantic-release/commit-analyzer',
            {
                preset: 'conventionalcommits',
                releaseRules: [
                    { type: 'build', release: false },
                    { type: 'chore', release: false },
                    { type: 'ci', release: false },
                    { type: 'docs', release: false },
                    { type: 'refactor', release: false },
                    { type: 'test', release: false },
                    { type: 'perf', release: 'patch' },
                ],
            },
        ],
        [
            '@semantic-release/release-notes-generator',
            {
                preset: 'conventionalcommits',
            },
        ],
        [
            '@semantic-release/exec',
            {
                prepareCmd: 'node scripts/release/prepare-release.mjs ${nextRelease.version}',
            },
        ],
        [
            '@semantic-release/github',
            {
                assets: [
                    {
                        path: 'artifacts/veritas-compose-*.tar.gz',
                        label: 'Docker Compose deployment bundle',
                    },
                    {
                        path: 'artifacts/veritas-compose-*.sha256',
                        label: 'Docker Compose bundle checksum',
                    },
                ],
            },
        ],
    ],
}
