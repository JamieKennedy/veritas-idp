import { beforeEach, describe, expect, it, vi } from 'vitest'

describe('deferSmtpSetup', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.unstubAllGlobals()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    it('persists the administrator decision through the SMTP deferral endpoint', async () => {
        const fetchMock = vi
            .fn()
            .mockResolvedValueOnce(new Response(JSON.stringify({ token: 'csrf-token' }), { headers: { 'Content-Type': 'application/json' } }))
            .mockResolvedValueOnce(new Response(null, { status: 204 }))
        vi.stubGlobal('fetch', fetchMock)
        const { deferSmtpSetup } = await import('./setup-status.api')

        await expect(deferSmtpSetup()).resolves.toBeUndefined()

        expect(fetchMock).toHaveBeenLastCalledWith('https://localhost:7100/api/v1/setup/smtp/defer', expect.objectContaining({ method: 'POST' }))
    })
})
