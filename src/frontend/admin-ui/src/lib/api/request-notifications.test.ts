import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('sonner', () => ({
    toast: {
        error: vi.fn(),
    },
}))

describe('request notifications', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.clearAllMocks()
        vi.unstubAllEnvs()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
        vi.stubGlobal('window', {})
    })

    it('notifies operational failures with a stable deduplication id', async () => {
        const { toast } = await import('sonner')
        const { ApiTransportError } = await import('./http-client')
        const { notifyOperationalRequestError } = await import('./request-notifications')

        notifyOperationalRequestError(new ApiTransportError())

        expect(toast.error).toHaveBeenCalledWith('Unable to connect to Veritas', {
            id: 'request-network-error',
            description: 'Check that the service is running and try again.',
        })
    })

    it('does not notify expected domain failures', async () => {
        const { toast } = await import('sonner')
        const { ApiProblem } = await import('./http-client')
        const { notifyOperationalRequestError } = await import('./request-notifications')

        notifyOperationalRequestError(new ApiProblem(401, 'Invalid administrator credentials.', 'AUTH_INVALID_CREDENTIALS', 'Unauthorized'))

        expect(toast.error).not.toHaveBeenCalled()
    })
})
