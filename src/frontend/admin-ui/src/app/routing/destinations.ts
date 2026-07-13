import type { AdminIdentity } from '@/features/auth/model/auth.schemas'
import type { SetupStatus } from '@/features/setup/model/setup-status.schema'

export type EntryDestination = '/bootstrap/complete' | '/bootstrap/start' | '/dashboard' | '/login'
export type LoginRedirect = '/bootstrap/smtp' | '/dashboard'

export function resolveEntryDestination(setup: SetupStatus, admin: AdminIdentity | null): EntryDestination {
    if (!setup.isConfigured) {
        return setup.hasActiveBootstrap ? '/bootstrap/complete' : '/bootstrap/start'
    }

    return admin === null ? '/login' : '/dashboard'
}

export function resolvePostLoginDestination(setup: SetupStatus, requestedDestination: LoginRedirect): LoginRedirect {
    return !setup.isSmtpConfigured && !setup.isSmtpSetupDeferred ? '/bootstrap/smtp' : requestedDestination
}

export function sanitizeLoginRedirect(value: unknown): LoginRedirect {
    return value === '/bootstrap/smtp' ? value : '/dashboard'
}
