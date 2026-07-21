import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'

import { resolveReleaseImages } from './image-names.mjs'

test('release image names use the repository owner and stable product names', () => {
    assert.deepEqual(
        resolveReleaseImages({
            githubRepository: 'JamieKennedy/veritas-idp',
        }),
        {
            adminUi: 'ghcr.io/jamiekennedy/veritas-admin-ui',
            backend: 'ghcr.io/jamiekennedy/veritas-backend',
        },
    )
})

test('release image names reject a malformed GitHub repository', () => {
    assert.throws(
        () =>
            resolveReleaseImages({
                githubRepository: 'veritas-idp',
            }),
        /owner\/repository/,
    )
})

test('Compose consumes every published application image', () => {
    const compose = readFileSync(new URL('../../compose.yaml', import.meta.url), 'utf8')
    const images = resolveReleaseImages({
        githubRepository: 'JamieKennedy/veritas-idp',
    })

    for (const image of Object.values(images)) {
        assert.ok(compose.includes(`${image}:`), `compose.yaml must consume ${image}`)
    }
})

test('Admin UI runtime serves the built client assets through srvx', () => {
    const dockerfile = readFileSync(new URL('../../src/frontend/admin-ui/Dockerfile', import.meta.url), 'utf8')
    const packageManifest = JSON.parse(readFileSync(new URL('../../src/frontend/admin-ui/package.json', import.meta.url), 'utf8'))
    const expectedCommand = 'srvx --prod --static ../dist/client server/entry.mjs'

    assert.equal(packageManifest.scripts.start, expectedCommand)
    assert.ok(
        dockerfile.includes('CMD ["./node_modules/.bin/srvx", "--prod", "--static", "../dist/client", "server/entry.mjs"]'),
        'The Admin UI container must expose dist/client through the supported srvx --static option.',
    )
})
