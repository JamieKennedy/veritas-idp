// @vitest-environment jsdom

import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { startAdminLogin } from '../api/auth.api'
import { CredentialsForm } from './credentials-form'
import type { LoginChallenge } from '../model/auth.schemas'

vi.mock('../api/auth.api', () => ({
    startAdminLogin: vi.fn(),
}))

vi.mock('@/lib/env/env', () => ({
    adminApiBaseUrl: 'https://localhost:7100',
}))

describe('CredentialsForm', () => {
    afterEach(() => {
        cleanup()
        vi.clearAllMocks()
    })

    it('validates through TanStack Form and submits a valid MFA challenge', async () => {
        const challenge: LoginChallenge = {
            challengeId: 'd34ec287-68ef-4ddc-861b-edc2f334bf84',
            challengeToken: 'short-lived-token',
            purpose: 'MfaVerification',
            expiresAtUtc: '2026-07-10T11:00:00Z',
            totpSecretBase32: null,
            totpProvisioningUri: null,
        }
        const onChallenge = vi.fn()
        vi.mocked(startAdminLogin).mockResolvedValue(challenge)
        render(<CredentialsForm onChallenge={onChallenge} />)

        fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

        expect(await screen.findByText('Enter a valid administrator email.')).not.toBeNull()
        expect(screen.getByText('Enter your password.')).not.toBeNull()
        expect(startAdminLogin).not.toHaveBeenCalled()

        fireEvent.change(screen.getByLabelText('Administrator email'), {
            target: { value: 'admin@example.com' },
        })
        fireEvent.change(screen.getByLabelText('Password'), {
            target: { value: 'correct horse battery staple' },
        })
        fireEvent.click(screen.getByRole('button', { name: 'Continue' }))

        await waitFor(() => {
            expect(onChallenge).toHaveBeenCalledWith(challenge)
        })
        expect(startAdminLogin).toHaveBeenCalledWith({
            email: 'admin@example.com',
            password: 'correct horse battery staple',
        })
    })
})
