import { ApiProblem, apiRequest } from '@/lib/api/http-client'
import { smtpSettingsSchema } from '../model/smtp.schemas'
import type { ConfigureSmtpInput } from '../model/smtp.schemas'

export async function getSmtpSettings(signal?: AbortSignal) {
    try {
        return await apiRequest('/api/v1/messaging/settings', { schema: smtpSettingsSchema, signal })
    } catch (error) {
        if (error instanceof ApiProblem && error.errorCode === 'MESSAGING_SMTP_NOT_CONFIGURED') {
            return {
                host: '',
                port: 587,
                tlsMode: 'StartTls' as const,
                username: null,
                hasSecret: false,
                fromEmail: '',
                fromName: null,
                isConfigured: false,
                lastSuccessfulTestAtUtc: null,
            }
        }

        throw error
    }
}

export function configureSmtp(input: ConfigureSmtpInput) {
    return apiRequest('/api/v1/messaging/settings/smtp', {
        method: 'PUT',
        body: {
            host: input.host,
            port: input.port,
            tlsMode: input.tlsMode,
            username: input.username === '' ? null : input.username,
            secret: input.secret === '' ? null : input.secret,
            fromEmail: input.fromEmail,
            fromName: input.fromName === '' ? null : input.fromName,
        },
        schema: smtpSettingsSchema,
    })
}
