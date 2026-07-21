import { MutationCache, QueryCache, QueryClient } from '@tanstack/react-query'

import { ApiProblem } from '@/lib/api/http-client'
import { notifyOperationalRequestError } from '@/lib/api/request-notifications'

function shouldRetryQuery(failureCount: number, error: unknown) {
    if (failureCount >= 2) {
        return false
    }

    if (error instanceof ApiProblem) {
        return error.status === 429 || error.status >= 500
    }

    return !(error instanceof DOMException && error.name === 'AbortError')
}

export function createQueryClient() {
    return new QueryClient({
        queryCache: new QueryCache({
            onError: (error, query) => {
                if (query.state.data !== undefined) {
                    notifyOperationalRequestError(error)
                }
            },
        }),
        mutationCache: new MutationCache({
            onError: (error) => {
                notifyOperationalRequestError(error)
            },
        }),
        defaultOptions: {
            queries: {
                retry: shouldRetryQuery,
            },
            mutations: {
                retry: false,
            },
        },
    })
}

export function getContext() {
    const queryClient = createQueryClient()

    return {
        queryClient,
    }
}
