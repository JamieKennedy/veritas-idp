import { queryOptions } from '@tanstack/react-query'

import { getSmtpSettings } from './smtp.api'

export const smtpSettingsQueryOptions = () =>
    queryOptions({
        queryKey: ['setup', 'smtp-settings'] as const,
        queryFn: ({ signal }) => getSmtpSettings(signal),
    })
