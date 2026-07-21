import assert from 'node:assert/strict'
import { describe, it } from 'node:test'

import { createServerHandler, parseAdminApiOrigin } from './proxy.mjs'

describe('Admin UI server proxy', () => {
    it('rejects unsafe upstream configuration', () => {
        for (const value of [undefined, '', 'ftp://admin-api', 'http://user:secret@admin-api', 'http://admin-api/path']) {
            assert.throws(() => parseAdminApiOrigin(value))
        }
    })

    it('serves a local health endpoint without calling the application', async () => {
        const application = rejectingApplication()
        const handler = createServerHandler({
            adminApiOrigin: new URL('http://admin-api:8080'),
            application,
        })

        const response = await handler.fetch(new Request('http://admin-ui:3000/healthz'))

        assert.equal(response.status, 204)
        assert.equal(response.headers.get('Cache-Control'), 'no-store')
    })

    it('delegates non-API requests to the application', async () => {
        const handler = createServerHandler({
            adminApiOrigin: new URL('http://admin-api:8080'),
            application: {
                fetch: () => new Response('application'),
            },
        })

        const response = await handler.fetch(new Request('http://admin-ui:3000/bootstrap/start'))

        assert.equal(await response.text(), 'application')
    })

    it('forwards API paths, queries, methods, bodies, and safe headers', async () => {
        /** @type {URL | undefined} */
        let forwardedUrl
        /** @type {RequestInit | undefined} */
        let forwardedInit
        const handler = createServerHandler({
            adminApiOrigin: new URL('http://admin-api:8080'),
            application: rejectingApplication(),
            fetchImplementation: async (url, init) => {
                forwardedUrl = new URL(url)
                forwardedInit = init
                return new Response('proxied', {
                    headers: {
                        'Set-Cookie': '__Host-veritas-admin=value; Secure; HttpOnly',
                    },
                })
            },
        })
        const request = new Request('https://veritas.example/api/v1/setup/status?include=smtp', {
            method: 'POST',
            headers: {
                Connection: 'keep-alive',
                'Content-Type': 'application/json',
                'X-Forwarded-Proto': 'https',
            },
            body: JSON.stringify({ enabled: true }),
        })

        const response = await handler.fetch(request)

        assert.equal(forwardedUrl?.toString(), 'http://admin-api:8080/api/v1/setup/status?include=smtp')
        assert.equal(forwardedInit?.method, 'POST')
        assert.equal(new Headers(forwardedInit?.headers).has('connection'), false)
        assert.equal(new Headers(forwardedInit?.headers).get('x-forwarded-host'), 'veritas.example')
        assert.equal(new Headers(forwardedInit?.headers).get('x-forwarded-proto'), 'https')
        assert.equal(await new Response(forwardedInit?.body).text(), '{"enabled":true}')
        assert.match(response.headers.get('Set-Cookie') ?? '', /__Host-veritas-admin/)
    })
})

function rejectingApplication() {
    return {
        fetch() {
            throw new Error('The application handler must not be called.')
        },
    }
}
