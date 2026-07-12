import { beforeEach, describe, expect, it, vi } from 'vitest'
import { z } from 'zod'

describe('admin API client', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.unstubAllGlobals()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    it('includes browser credentials and validates successful JSON responses', async () => {
        const fetchMock = vi.fn().mockResolvedValue(
            new Response(JSON.stringify({ value: 'ok' }), {
                status: 200,
                headers: { 'Content-Type': 'application/json' },
            }),
        )
        vi.stubGlobal('fetch', fetchMock)
        const { apiRequest } = await import('./http-client')

        const result = await apiRequest('/api/v1/example', { schema: z.object({ value: z.string() }) })

        expect(result).toEqual({ value: 'ok' })
        expect(fetchMock).toHaveBeenCalledWith('https://localhost:7100/api/v1/example', expect.objectContaining({ credentials: 'include' }))
    })

    it('acquires CSRF and attaches it to unsafe requests', async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValueOnce(
                new Response(JSON.stringify({ token: 'csrf-token' }), {
                    status: 200,
                    headers: { 'Content-Type': 'application/json' },
                }),
            )
            .mockResolvedValueOnce(new Response(null, { status: 200 }))
        vi.stubGlobal('fetch', fetchMock)
        const { apiRequest } = await import('./http-client')

        await apiRequest('/api/v1/example', { method: 'POST', body: { secret: 'not-logged' } })

        expect(fetchMock.mock.calls[1]?.[0]).toBe('https://localhost:7100/api/v1/example')
        const request = fetchMock.mock.calls[1]?.[1] as RequestInit
        expect(request.credentials).toBe('include')
        expect(new Headers(request.headers).get('X-CSRF-TOKEN')).toBe('csrf-token')
    })

    it('reacquires CSRF after the authenticated principal changes', async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValueOnce(new Response(JSON.stringify({ token: 'anonymous-token' }), { status: 200 }))
            .mockResolvedValueOnce(new Response(null, { status: 200 }))
            .mockResolvedValueOnce(new Response(JSON.stringify({ token: 'admin-token' }), { status: 200 }))
            .mockResolvedValueOnce(new Response(null, { status: 200 }))
        vi.stubGlobal('fetch', fetchMock)
        const { apiRequest, resetCsrfToken } = await import('./http-client')

        await apiRequest('/api/v1/admin-auth/login', { method: 'POST', body: {} })
        resetCsrfToken()
        await apiRequest('/api/v1/messaging/settings/smtp', { method: 'PUT', body: {} })

        const authenticatedRequest = fetchMock.mock.calls[3]?.[1] as RequestInit
        expect(new Headers(authenticatedRequest.headers).get('X-CSRF-TOKEN')).toBe('admin-token')
    })

    it('throws a safe API problem for unsuccessful responses', async () => {
        vi.stubGlobal(
            'fetch',
            vi.fn().mockResolvedValue(
                new Response(JSON.stringify({ title: 'Request failed.', detail: 'Safe detail.', status: 409 }), {
                    status: 409,
                    headers: { 'Content-Type': 'application/problem+json' },
                }),
            ),
        )
        const { apiRequest } = await import('./http-client')

        await expect(apiRequest('/api/v1/example')).rejects.toEqual(expect.objectContaining({ name: 'ApiProblem', status: 409, message: 'Safe detail.' }))
    })

    it('wraps fetch failures as transport errors', async () => {
        vi.stubGlobal('fetch', vi.fn().mockRejectedValue(new TypeError('fetch failed')))
        const { ApiTransportError, apiRequest } = await import('./http-client')

        await expect(apiRequest('/api/v1/example')).rejects.toBeInstanceOf(ApiTransportError)
    })

    it('wraps invalid successful responses as contract errors', async () => {
        vi.stubGlobal('fetch', vi.fn().mockResolvedValue(new Response('{not-json', { status: 200 })))
        const { ApiContractError, apiRequest } = await import('./http-client')

        await expect(apiRequest('/api/v1/example')).rejects.toBeInstanceOf(ApiContractError)
    })
})
