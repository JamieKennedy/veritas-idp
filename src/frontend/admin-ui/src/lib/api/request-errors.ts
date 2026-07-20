import { ApiProblem, ApiTransportError } from './http-client'

export interface OperationalToast {
    id: string
    title: string
    description: string
}

/**
 * Returns a message suitable for an inline form error when the server rejected a valid request.
 * Operational failures are intentionally omitted so they can be presented by the global request pipeline.
 */
export function getInlineRequestError(error: unknown): string | undefined {
    if (!(error instanceof ApiProblem) || isOperationalProblem(error)) {
        return undefined
    }

    return error.message
}

/**
 * Classifies failures that should be shown through the global operational error notification.
 */
export function getOperationalToast(error: unknown): OperationalToast | undefined {
    if (isAbortError(error)) {
        return undefined
    }

    if (error instanceof ApiTransportError) {
        return {
            id: 'request-network-error',
            title: 'Unable to connect to Veritas',
            description: 'Check that the service is running and try again.',
        }
    }

    if (error instanceof ApiProblem) {
        if (error.status === 400 && !error.errorCode) {
            return {
                id: 'request-rejected',
                title: 'Request could not be verified',
                description: 'Refresh the page and try again.',
            }
        }

        if (error.status === 429) {
            return {
                id: 'request-rate-limit',
                title: 'Too many attempts',
                description: 'Wait a few minutes and try again.',
            }
        }

        if (error.status >= 500) {
            return {
                id: 'request-server-error',
                title: 'Veritas is temporarily unavailable',
                description: 'Try again in a moment.',
            }
        }

        return undefined
    }

    return {
        id: 'request-unexpected-error',
        title: 'Unexpected response',
        description: 'Refresh the page and try again.',
    }
}

function isOperationalProblem(error: ApiProblem): boolean {
    return (error.status === 400 && !error.errorCode) || error.status === 429 || error.status >= 500
}

function isAbortError(error: unknown): boolean {
    return error instanceof DOMException && error.name === 'AbortError'
}
