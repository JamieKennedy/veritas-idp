import { beforeEach, describe, expect, it, vi } from 'vitest'

describe('parseAdminUiEnv', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
    })

    it('normalizes the configured Admin API base URL', async () => {
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
        const { parseAdminUiEnv } = await import('./env')

        const config = parseAdminUiEnv({
            VITE_ADMIN_API_BASE_URL: 'https://localhost:7100/',
        } as ImportMetaEnv)

        expect(config.adminApiBaseUrl).toBe('https://localhost:7100')
    })

    it('rejects an invalid Admin API base URL', async () => {
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
        const { parseAdminUiEnv } = await import('./env')

        expect(() =>
            parseAdminUiEnv({
                VITE_ADMIN_API_BASE_URL: 'not-a-url',
            } as ImportMetaEnv),
        ).toThrow()
    })
})
