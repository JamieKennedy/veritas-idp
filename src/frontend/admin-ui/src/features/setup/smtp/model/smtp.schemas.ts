import { z } from 'zod'

export const smtpTlsModeSchema = z.enum(['None', 'StartTls'])

export const smtpSettingsSchema = z.object({
    host: z.string(),
    port: z.number().int(),
    tlsMode: smtpTlsModeSchema,
    username: z.string().nullable(),
    hasSecret: z.boolean(),
    fromEmail: z.string(),
    fromName: z.string().nullable(),
    isConfigured: z.boolean(),
    lastSuccessfulTestAtUtc: z.iso.datetime({ offset: true }).nullable(),
})

export const configureSmtpSchema = z.object({
    host: z.string().trim().min(1, 'Enter the SMTP host.'),
    port: z.number().int().min(1).max(65_535),
    tlsMode: smtpTlsModeSchema,
    username: z.string().trim(),
    secret: z.string(),
    fromEmail: z.email('Enter a valid sender email.'),
    fromName: z.string().trim(),
})

export type ConfigureSmtpInput = z.infer<typeof configureSmtpSchema>
export type SmtpSettings = z.infer<typeof smtpSettingsSchema>
