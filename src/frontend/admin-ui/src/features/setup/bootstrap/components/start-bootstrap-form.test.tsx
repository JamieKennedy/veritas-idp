// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { StartBootstrapForm } from './start-bootstrap-form'

const { mockInvalidate, mockNavigate, mockStartBootstrap } = vi.hoisted(() => ({
    mockInvalidate: vi.fn(),
    mockNavigate: vi.fn(),
    mockStartBootstrap: vi.fn(),
}))

vi.mock('@tanstack/react-router', () => ({
    useNavigate: () => mockNavigate,
    useRouter: () => ({ invalidate: mockInvalidate }),
}))

vi.mock('../api/bootstrap.api', () => ({
    startBootstrapMutationOptions: () => ({ mutationFn: mockStartBootstrap }),
}))

vi.mock('@/lib/env/env', () => ({
    adminApiBaseUrl: 'https://localhost:7100',
}))

describe('StartBootstrapForm', () => {
    afterEach(() => {
        cleanup()
        vi.clearAllMocks()
    })

    it('refreshes route context before navigating after an empty successful response', async () => {
        mockStartBootstrap.mockResolvedValue(undefined)
        mockInvalidate.mockResolvedValue(undefined)
        mockNavigate.mockResolvedValue(undefined)
        const queryClient = new QueryClient()
        render(
            <QueryClientProvider client={queryClient}>
                <StartBootstrapForm />
            </QueryClientProvider>,
        )

        fireEvent.change(screen.getByLabelText('First administrator email'), { target: { value: 'admin@example.com' } })
        fireEvent.change(screen.getByLabelText('Deployment bootstrap secret'), { target: { value: 'bootstrap-secret' } })
        fireEvent.click(screen.getByRole('button', { name: 'Verify and continue' }))

        await waitFor(() => {
            expect(mockInvalidate).toHaveBeenCalledOnce()
            expect(mockNavigate).toHaveBeenCalledWith({ to: '/bootstrap/complete' })
        })
        expect(mockInvalidate.mock.invocationCallOrder[0]).toBeLessThan(mockNavigate.mock.invocationCallOrder[0] ?? Number.POSITIVE_INFINITY)
    })
})
