import { beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('@/lib/api/request-notifications', () => ({
    notifyOperationalRequestError: vi.fn(),
}))

describe('React Query request error pipeline', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    it('does not toast a failure when an initial query has no data', async () => {
        const { notifyOperationalRequestError } = await import('@/lib/api/request-notifications')
        const { ApiTransportError } = await import('@/lib/api/http-client')
        const { createQueryClient } = await import('./root-provider')
        const queryClient = createQueryClient()

        await expect(
            queryClient.fetchQuery({
                queryKey: ['initial'],
                queryFn: () => Promise.reject(new ApiTransportError()),
                retry: false,
            }),
        ).rejects.toBeInstanceOf(ApiTransportError)

        expect(notifyOperationalRequestError).not.toHaveBeenCalled()
    })

    it('toasts operational failures when a background query already has data', async () => {
        const { notifyOperationalRequestError } = await import('@/lib/api/request-notifications')
        const { ApiTransportError } = await import('@/lib/api/http-client')
        const { createQueryClient } = await import('./root-provider')
        const queryClient = createQueryClient()
        queryClient.setQueryData(['background'], { ready: true })

        await expect(
            queryClient.fetchQuery({
                queryKey: ['background'],
                queryFn: () => Promise.reject(new ApiTransportError()),
                retry: false,
            }),
        ).rejects.toBeInstanceOf(ApiTransportError)

        expect(notifyOperationalRequestError).toHaveBeenCalledWith(expect.any(ApiTransportError))
    })
})
