import { getInlineRequestError } from './request-errors'

export function getSafeErrorMessage(error: unknown): string {
    return getInlineRequestError(error) ?? 'The request could not be completed. Please try again.'
}
