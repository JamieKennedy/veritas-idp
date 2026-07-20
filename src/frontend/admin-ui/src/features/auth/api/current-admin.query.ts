import { queryOptions } from '@tanstack/react-query'

import { getCurrentAdmin } from './auth.api'

export const currentAdminQueryOptions = () =>
    queryOptions({
        queryKey: ['auth', 'current-admin'] as const,
        queryFn: ({ signal }) => getCurrentAdmin(signal),
        staleTime: 10_000,
        retry: false,
    })
