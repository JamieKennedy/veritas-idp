import { ApiProblem } from './http-client'

export function getSafeErrorMessage(error: unknown): string {
    return error instanceof ApiProblem ? error.message : 'The request could not be completed. Please try again.'
}
