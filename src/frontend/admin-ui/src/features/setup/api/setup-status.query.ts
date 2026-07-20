import { queryOptions } from '@tanstack/react-query'

import { getSetupStatus } from './setup-status.api'

export const setupStatusQueryOptions = () =>
    queryOptions({
        queryKey: ['setup', 'status'] as const,
        queryFn: ({ signal }) => getSetupStatus(signal),
        staleTime: 5_000,
    })
