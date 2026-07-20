// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { cleanup, fireEvent, render, screen, within } from '@testing-library/react'
import { afterEach, describe, expect, it, vi } from 'vitest'
import type { Ref } from 'react'

import { MfaEnrollment } from './mfa-enrollment'
import type { LoginChallenge } from '../model/auth.schemas'

vi.mock('@/lib/env/env', () => ({
    adminApiBaseUrl: 'https://localhost:7100',
}))

vi.mock('qrcode.react', () => ({
    QRCodeCanvas: ({ ref, title, value }: { ref?: Ref<HTMLCanvasElement>; title?: string; value: string }) => (
        <canvas ref={ref} title={title} data-value={value} />
    ),
}))

const challenge: LoginChallenge = {
    challengeId: '5bc2e151-b36a-4698-a273-90ecf4b5aa7f',
    challengeToken: 'one-time-token',
    purpose: 'MfaEnrollment',
    expiresAtUtc: '2026-07-13T23:00:00Z',
    totpSecretBase32: 'JBSWY3DPEHPK3PXP',
    totpProvisioningUri: 'otpauth://totp/Veritas%3Aadmin%40example.com?secret=JBSWY3DPEHPK3PXP&issuer=Veritas&digits=6&period=30',
}

describe('MfaEnrollment', () => {
    afterEach(() => {
        cleanup()
        vi.restoreAllMocks()
    })

    it('shows all manual enrollment metadata in a collapsed advanced disclosure', () => {
        renderEnrollment(challenge)

        const summary = screen.getByText('Advanced')
        const disclosure = summary.closest('details')
        if (!disclosure) {
            throw new Error('Expected the Advanced summary to be inside a details element.')
        }

        expect(disclosure.open).toBe(false)

        fireEvent.click(summary)

        expect(disclosure.open).toBe(true)
        const details = within(disclosure)
        expect(details.getByText('Time-based (TOTP)')).toBeTruthy()
        expect(details.getByText('Base32')).toBeTruthy()
        expect(details.getByText('SHA-1')).toBeTruthy()
        expect(details.getByText('6')).toBeTruthy()
        expect(details.getByText('30 seconds')).toBeTruthy()
        expect(details.getByText('Veritas')).toBeTruthy()
        expect(details.getByText('admin@example.com')).toBeTruthy()
    })

    it('downloads the generated QR code as a PNG without submitting enrollment', () => {
        renderEnrollment(challenge)
        const pngDataUrl = 'data:image/png;base64,generated-qr-code'
        vi.spyOn(HTMLCanvasElement.prototype, 'toDataURL').mockReturnValue(pngDataUrl)
        const anchor = document.createElement('a')
        const anchorClick = vi.spyOn(anchor, 'click').mockImplementation(() => undefined)
        vi.spyOn(document, 'createElement').mockReturnValueOnce(anchor)
        const fetchSpy = vi.spyOn(globalThis, 'fetch')

        const downloadButton = screen.getByRole('button', { name: 'Download QR code' })
        expect(downloadButton.getAttribute('type')).toBe('button')
        fireEvent.click(downloadButton)

        expect(anchor.download).toBe('veritas-totp-qr.png')
        expect(anchor.href).toBe(pngDataUrl)
        expect(anchorClick).toHaveBeenCalledOnce()
        expect(fetchSpy).not.toHaveBeenCalled()
    })

    it('omits QR download and advanced metadata when provisioning data is unavailable', () => {
        renderEnrollment({ ...challenge, totpProvisioningUri: null })

        expect(screen.queryByRole('button', { name: 'Download QR code' })).toBeNull()
        expect(screen.queryByText('Advanced')).toBeNull()
    })
})

function renderEnrollment(enrollmentChallenge: LoginChallenge) {
    const queryClient = new QueryClient({ defaultOptions: { mutations: { retry: false } } })

    return render(
        <QueryClientProvider client={queryClient}>
            <MfaEnrollment challenge={enrollmentChallenge} onComplete={vi.fn()} onRejected={vi.fn()} onReset={vi.fn()} />
        </QueryClientProvider>,
    )
}
