import { describe, expect, it } from 'vitest'

import { resolveEntryDestination, sanitizeLoginRedirect } from './destinations'

describe('resolveEntryDestination', () => {
    it.each([
        {
            name: 'resumes an active bootstrap session',
            setup: { isConfigured: false, hasActiveBootstrap: true, activeBootstrapExpiresAtUtc: null, isSmtpConfigured: false },
            admin: null,
            expected: '/bootstrap/complete',
        },
        {
            name: 'starts bootstrap for a new installation',
            setup: { isConfigured: false, hasActiveBootstrap: false, activeBootstrapExpiresAtUtc: null, isSmtpConfigured: false },
            admin: null,
            expected: '/bootstrap/start',
        },
        {
            name: 'sends an unauthenticated configured installation to login',
            setup: { isConfigured: true, hasActiveBootstrap: false, activeBootstrapExpiresAtUtc: null, isSmtpConfigured: false },
            admin: null,
            expected: '/login',
        },
        {
            name: 'allows an authenticated administrator into the dashboard when SMTP is incomplete',
            setup: { isConfigured: true, hasActiveBootstrap: false, activeBootstrapExpiresAtUtc: null, isSmtpConfigured: false },
            admin: { id: '5bc2e151-b36a-4698-a273-90ecf4b5aa7f', email: 'admin@example.com', name: 'First Admin' },
            expected: '/dashboard',
        },
    ])('$name', ({ setup, admin, expected }) => {
        expect(resolveEntryDestination(setup, admin)).toBe(expected)
    })
})

describe('sanitizeLoginRedirect', () => {
    it('accepts the first-run SMTP continuation', () => {
        expect(sanitizeLoginRedirect('/bootstrap/smtp')).toBe('/bootstrap/smtp')
    })

    it.each([undefined, '', 'https://example.com', '//example.com', '/bootstrap/start'])('defaults %s to the dashboard', (value) => {
        expect(sanitizeLoginRedirect(value)).toBe('/dashboard')
    })
})
