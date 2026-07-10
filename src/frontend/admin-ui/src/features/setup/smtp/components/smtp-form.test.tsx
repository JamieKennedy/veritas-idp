// @vitest-environment jsdom

import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'

import { configureSmtp } from '../api/smtp.api'
import { SmtpForm } from './smtp-form'
import type { SmtpSettings } from '../model/smtp.schemas'

vi.mock('../api/smtp.api', () => ({
    configureSmtp: vi.fn(),
}))

vi.mock('@/lib/env/env', () => ({
    adminApiBaseUrl: 'https://localhost:7100',
}))

const emptySettings: SmtpSettings = {
    host: '',
    port: 587,
    tlsMode: 'StartTls',
    username: null,
    hasSecret: false,
    fromEmail: '',
    fromName: null,
    isConfigured: false,
    lastSuccessfulTestAtUtc: null,
}

describe('SmtpForm', () => {
    afterEach(() => {
        cleanup()
        vi.clearAllMocks()
    })

    it('validates and submits SMTP settings through TanStack Form', async () => {
        const onConfigured = vi.fn()
        vi.mocked(configureSmtp).mockResolvedValue({ ...emptySettings, host: 'smtp.example.com', fromEmail: 'veritas@example.com', isConfigured: true })
        render(<SmtpForm settings={emptySettings} onConfigured={onConfigured} />)

        fireEvent.click(screen.getByRole('button', { name: 'Save and test connection' }))

        expect(await screen.findByText('Enter the SMTP host.')).not.toBeNull()
        expect(screen.getByText('Enter a valid sender email.')).not.toBeNull()
        expect(configureSmtp).not.toHaveBeenCalled()

        fireEvent.change(screen.getByLabelText('Host'), { target: { value: 'smtp.example.com' } })
        fireEvent.change(screen.getByLabelText('From email'), { target: { value: 'veritas@example.com' } })
        fireEvent.click(screen.getByRole('button', { name: 'Save and test connection' }))

        await waitFor(() => {
            expect(onConfigured).toHaveBeenCalledOnce()
        })
        expect(configureSmtp).toHaveBeenCalledWith({
            host: 'smtp.example.com',
            port: 587,
            tlsMode: 'StartTls',
            username: '',
            secret: '',
            fromEmail: 'veritas@example.com',
            fromName: '',
        })
    })
})
