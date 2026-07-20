// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type * as TanStackReactRouter from '@tanstack/react-router'

import { SmtpSetupPage } from './smtp-setup-page'

const { mockDeferSmtpSetup, mockGetSetupStatus, mockGetSmtpSettings, mockInvalidate, mockNavigate } = vi.hoisted(() => ({
    mockDeferSmtpSetup: vi.fn(),
    mockGetSetupStatus: vi.fn(),
    mockGetSmtpSettings: vi.fn(),
    mockInvalidate: vi.fn(),
    mockNavigate: vi.fn(),
}))

vi.mock('@tanstack/react-router', async (importOriginal) => ({
    ...(await importOriginal<typeof TanStackReactRouter>()),
    useNavigate: () => mockNavigate,
    useRouter: () => ({ invalidate: mockInvalidate }),
}))

vi.mock('../../api/setup-status.api', () => ({
    deferSmtpSetupMutationOptions: () => ({ mutationFn: mockDeferSmtpSetup }),
}))

vi.mock('../../api/setup-status.query', () => ({
    setupStatusQueryOptions: () => ({
        queryKey: ['setup', 'status'],
        queryFn: mockGetSetupStatus,
    }),
}))

vi.mock('../api/smtp-settings.query', () => ({
    smtpSettingsQueryOptions: () => ({
        queryKey: ['setup', 'smtp-settings'],
        queryFn: mockGetSmtpSettings,
    }),
}))

vi.mock('../components/smtp-form', () => ({
    SmtpForm: ({ onConfigured }: { onConfigured: () => void }) => <button onClick={onConfigured}>Finish SMTP</button>,
}))

vi.mock('../components/smtp-skip-dialog', () => ({
    SmtpSkipDialog: ({ onSkip }: { onSkip: () => void }) => <button onClick={onSkip}>Skip SMTP</button>,
}))

describe('SmtpSetupPage', () => {
    afterEach(() => {
        cleanup()
        vi.clearAllMocks()
    })

    it('refreshes setup route state after SMTP configuration', async () => {
        prepareSuccessfulSetupTransition()
        const queryClient = new QueryClient()
        render(
            <QueryClientProvider client={queryClient}>
                <SmtpSetupPage />
            </QueryClientProvider>,
        )

        fireEvent.click(await screen.findByRole('button', { name: 'Finish SMTP' }))

        await waitFor(() => {
            expect(mockGetSetupStatus).toHaveBeenCalledOnce()
            expect(mockInvalidate).toHaveBeenCalledOnce()
            expect(mockNavigate).toHaveBeenCalledWith({ to: '/dashboard' })
        })
    })

    it('refreshes setup route state after SMTP deferral', async () => {
        prepareSuccessfulSetupTransition()
        mockDeferSmtpSetup.mockResolvedValue(undefined)
        const queryClient = new QueryClient()
        render(
            <QueryClientProvider client={queryClient}>
                <SmtpSetupPage />
            </QueryClientProvider>,
        )

        fireEvent.click(await screen.findByRole('button', { name: 'Skip SMTP' }))

        await waitFor(() => {
            expect(mockGetSetupStatus).toHaveBeenCalledOnce()
            expect(mockInvalidate).toHaveBeenCalledOnce()
            expect(mockNavigate).toHaveBeenCalledWith({ to: '/dashboard' })
        })
    })
})

function prepareSuccessfulSetupTransition() {
    mockGetSetupStatus.mockResolvedValue({
        isConfigured: true,
        hasActiveBootstrap: false,
        activeBootstrapExpiresAtUtc: null,
        isSmtpConfigured: true,
        isSmtpSetupDeferred: false,
    })
    mockGetSmtpSettings.mockResolvedValue({
        host: '',
        port: 587,
        tlsMode: 'StartTls',
        username: null,
        hasSecret: false,
        fromEmail: '',
        fromName: null,
        isConfigured: false,
        lastSuccessfulTestAtUtc: null,
    })
    mockInvalidate.mockResolvedValue(undefined)
    mockNavigate.mockResolvedValue(undefined)
}
