// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type * as TanStackReactRouter from '@tanstack/react-router'

import { LoginPage } from './login-page'
import type { LoginChallenge } from '../model/auth.schemas'

const { mockGetCurrentAdmin, mockGetSetupStatus, mockNavigate } = vi.hoisted(() => ({
    mockGetCurrentAdmin: vi.fn(),
    mockGetSetupStatus: vi.fn(),
    mockNavigate: vi.fn(),
}))

vi.mock('@tanstack/react-router', async (importOriginal) => ({
    ...(await importOriginal<typeof TanStackReactRouter>()),
    useNavigate: () => mockNavigate,
}))

vi.mock('../components/credentials-form', () => ({
    CredentialsForm: ({ onChallenge }: { onChallenge: (challenge: LoginChallenge) => void }) => (
        <button
            onClick={() => {
                onChallenge({
                    challengeId: 'valid-challenge',
                    challengeToken: 'short-lived-token',
                    purpose: 'MfaVerification',
                    expiresAtUtc: '2026-07-13T13:00:00Z',
                    totpSecretBase32: null,
                    totpProvisioningUri: null,
                })
            }}
        >
            Start MFA
        </button>
    ),
}))

vi.mock('../components/mfa-verification', () => ({
    MfaVerification: ({ onComplete }: { onComplete: () => void }) => <button onClick={onComplete}>Complete MFA</button>,
}))

vi.mock('@/features/setup/api/setup-status.query', () => ({
    setupStatusQueryOptions: () => ({
        queryKey: ['setup', 'status'],
        queryFn: mockGetSetupStatus,
    }),
}))

vi.mock('../api/current-admin.query', () => ({
    currentAdminQueryOptions: () => ({
        queryKey: ['auth', 'current-admin'],
        queryFn: mockGetCurrentAdmin,
    }),
}))

vi.mock('@/lib/env/env', () => ({
    adminApiBaseUrl: 'https://localhost:7100',
}))

describe('LoginPage', () => {
    afterEach(() => {
        cleanup()
        vi.clearAllMocks()
    })

    it('uses refreshed setup and administrator state to continue to SMTP setup', async () => {
        mockGetCurrentAdmin.mockResolvedValue({
            id: 'admin-id',
            email: 'admin@example.com',
            name: 'Admin',
        })
        mockGetSetupStatus.mockResolvedValue({
            isConfigured: true,
            hasActiveBootstrap: false,
            activeBootstrapExpiresAtUtc: null,
            isSmtpConfigured: false,
            isSmtpSetupDeferred: false,
        })
        mockNavigate.mockResolvedValue(undefined)
        const queryClient = new QueryClient()
        render(
            <QueryClientProvider client={queryClient}>
                <LoginPage redirectTo="/dashboard" />
            </QueryClientProvider>,
        )

        fireEvent.click(screen.getByRole('button', { name: 'Start MFA' }))
        fireEvent.click(screen.getByRole('button', { name: 'Complete MFA' }))

        await waitFor(() => {
            expect(mockGetCurrentAdmin).toHaveBeenCalledOnce()
            expect(mockGetSetupStatus).toHaveBeenCalledOnce()
            expect(mockNavigate).toHaveBeenCalledWith({ to: '/bootstrap/smtp' })
        })
    })
})
