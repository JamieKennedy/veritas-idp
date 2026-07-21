import { mutationOptions } from '@tanstack/react-query'

import { apiRequest } from '@/lib/api/http-client'
import type { CompleteBootstrapInput, StartBootstrapInput } from '../model/bootstrap.schemas'

export function startBootstrap(input: StartBootstrapInput) {
    return apiRequest('/api/v1/bootstrap/start', {
        method: 'POST',
        body: { email: input.email, bootstrapSecret: input.bootstrapSecret },
    })
}

export function completeBootstrap(input: CompleteBootstrapInput) {
    return apiRequest('/api/v1/bootstrap/complete', {
        method: 'POST',
        body: { password: input.password, displayName: input.displayName === '' ? null : input.displayName },
    })
}

export const startBootstrapMutationOptions = () =>
    mutationOptions({
        mutationFn: startBootstrap,
    })

export const completeBootstrapMutationOptions = () =>
    mutationOptions({
        mutationFn: completeBootstrap,
    })
