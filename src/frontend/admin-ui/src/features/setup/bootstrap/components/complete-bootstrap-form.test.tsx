// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type * as TanStackReactRouter from '@tanstack/react-router'

import { CompleteBootstrapForm } from './complete-bootstrap-form'

const { mockCompleteBootstrap, mockGetSetupStatus, mockNavigate } = vi.hoisted(() => ({
    mockCompleteBootstrap: vi.fn(),
    mockGetSetupStatus: vi.fn(),
    mockNavigate: vi.fn(),
}))

vi.mock('@tanstack/react-router', async (importOriginal) => ({
    ...(await importOriginal<typeof TanStackReactRouter>()),
    useNavigate: () => mockNavigate,
}))

vi.mock('../api/bootstrap.api', () => ({
    completeBootstrapMutationOptions: () => ({ mutationFn: mockCompleteBootstrap }),
}))

vi.mock('../../api/setup-status.query', () => ({
    setupStatusQueryOptions: () => ({
        queryKey: ['setup', 'status'],
        queryFn: mockGetSetupStatus,
    }),
}))

vi.mock('@/lib/env/env', () => ({
    adminApiBaseUrl: 'https://localhost:7100',
}))

describe('CompleteBootstrapForm', () => {
    afterEach(() => {
        cleanup()
        vi.clearAllMocks()
    })

    it('refreshes setup status before opening the first administrator sign-in', async () => {
        mockCompleteBootstrap.mockResolvedValue(undefined)
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
                <CompleteBootstrapForm />
            </QueryClientProvider>,
        )

        fireEvent.change(screen.getByLabelText('Password'), { target: { value: 'correct horse battery staple' } })
        fireEvent.change(screen.getByLabelText('Confirm password'), { target: { value: 'correct horse battery staple' } })
        fireEvent.click(screen.getByRole('button', { name: 'Create administrator' }))

        await waitFor(() => {
            expect(mockGetSetupStatus).toHaveBeenCalledOnce()
            expect(mockNavigate).toHaveBeenCalledWith({ to: '/login', search: { redirect: '/bootstrap/smtp' } })
        })
    })
})
