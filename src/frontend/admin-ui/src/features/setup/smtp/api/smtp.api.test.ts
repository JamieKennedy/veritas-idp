import { beforeEach, describe, expect, it, vi } from 'vitest'

describe('getSmtpSettings', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.unstubAllGlobals()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    it('returns safe defaults when SMTP has never been configured', async () => {
        vi.stubGlobal(
            'fetch',
            vi.fn().mockResolvedValue(
                new Response(
                    JSON.stringify({
                        status: 409,
                        title: 'Conflict.',
                        detail: 'SMTP must be configured before email can be sent.',
                        errorCode: 'MESSAGING_SMTP_NOT_CONFIGURED',
                    }),
                    { status: 409, headers: { 'Content-Type': 'application/problem+json' } },
                ),
            ),
        )
        const { getSmtpSettings } = await import('./smtp.api')

        await expect(getSmtpSettings()).resolves.toEqual({
            host: '',
            port: 587,
            tlsMode: 'StartTls',
            username: null,
            hasSecret: false,
            fromEmail: '',
            fromName: null,
            isConfigured: false,
            lastSuccessfulTestAtUtc: null,
        })
    })
})
