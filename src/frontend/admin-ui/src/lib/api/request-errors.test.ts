import { beforeEach, describe, expect, it, vi } from 'vitest'

describe('request error policy', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    it('keeps typed domain failures available to the submitting form', async () => {
        const { getInlineRequestError, getOperationalToast } = await import('./request-errors')
        const { ApiProblem } = await import('./http-client')
        const error = new ApiProblem(401, 'The provided bootstrap secret is invalid.', 'PLATFORM_BOOTSTRAP_SECRET_INVALID', 'Unauthorized')

        expect(getInlineRequestError(error)).toBe('The provided bootstrap secret is invalid.')
        expect(getOperationalToast(error)).toBeUndefined()
    })

    it('routes transport failures to an operational toast', async () => {
        const { getInlineRequestError, getOperationalToast } = await import('./request-errors')
        const { ApiTransportError } = await import('./http-client')
        const toast = getOperationalToast(new ApiTransportError())

        expect(toast).toEqual({
            id: 'request-network-error',
            title: 'Unable to connect to Veritas',
            description: 'Check that the service is running and try again.',
        })
        expect(getInlineRequestError(new ApiTransportError())).toBeUndefined()
    })

    it('routes bare bad requests to a toast instead of form validation', async () => {
        const { getInlineRequestError, getOperationalToast } = await import('./request-errors')
        const { ApiProblem } = await import('./http-client')
        const error = new ApiProblem(400, 'Bad Request')

        expect(getInlineRequestError(error)).toBeUndefined()
        expect(getOperationalToast(error)?.id).toBe('request-rejected')
    })

    it('routes server failures and rate limits to operational toasts', async () => {
        const { getOperationalToast } = await import('./request-errors')
        const { ApiProblem } = await import('./http-client')
        expect(getOperationalToast(new ApiProblem(429, 'Too many requests'))?.id).toBe('request-rate-limit')
        expect(getOperationalToast(new ApiProblem(503, 'Unavailable'))?.id).toBe('request-server-error')
    })

    it('does not toast cancelled requests', async () => {
        const { getInlineRequestError, getOperationalToast } = await import('./request-errors')
        const error = new DOMException('The request was aborted.', 'AbortError')

        expect(getOperationalToast(error)).toBeUndefined()
        expect(getInlineRequestError(error)).toBeUndefined()
    })
})
