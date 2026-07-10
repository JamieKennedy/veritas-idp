import { z } from 'zod'

export const setupStatusSchema = z.object({
    isConfigured: z.boolean(),
    hasActiveBootstrap: z.boolean(),
    activeBootstrapExpiresAtUtc: z.iso.datetime({ offset: true }).nullable(),
    isSmtpConfigured: z.boolean(),
})

export type SetupStatus = z.infer<typeof setupStatusSchema>
