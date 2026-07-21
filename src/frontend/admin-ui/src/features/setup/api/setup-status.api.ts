import { mutationOptions } from '@tanstack/react-query'

import { apiRequest } from '@/lib/api/http-client'
import { setupStatusSchema } from '../model/setup-status.schema'

export function getSetupStatus(signal?: AbortSignal) {
    return apiRequest('/api/v1/setup/status', { schema: setupStatusSchema, signal })
}

export function deferSmtpSetup() {
    return apiRequest('/api/v1/setup/smtp/defer', { method: 'POST' })
}

export const deferSmtpSetupMutationOptions = () =>
    mutationOptions({
        mutationFn: deferSmtpSetup,
    })
