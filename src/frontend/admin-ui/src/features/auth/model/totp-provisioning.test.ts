import { describe, expect, it } from 'vitest'

import { parseTotpProvisioningUri } from './totp-provisioning'

describe('parseTotpProvisioningUri', () => {
    it('reads enrollment identity and applies standard TOTP defaults', () => {
        expect(parseTotpProvisioningUri('otpauth://totp/Veritas%3Aadmin%40example.com?secret=JBSWY3DPEHPK3PXP&issuer=Veritas')).toEqual({
            type: 'Time-based (TOTP)',
            secretFormat: 'Base32',
            algorithm: 'SHA-1',
            digits: 6,
            periodSeconds: 30,
            issuer: 'Veritas',
            account: 'admin@example.com',
        })
    })

    it('uses explicit algorithm, digit, and period parameters', () => {
        const details = parseTotpProvisioningUri(
            'otpauth://totp/Veritas%3Aadmin%40example.com?secret=JBSWY3DPEHPK3PXP&issuer=Veritas&algorithm=SHA256&digits=8&period=60',
        )

        expect(details).toMatchObject({ algorithm: 'SHA-256', digits: 8, periodSeconds: 60 })
    })

    it('falls back to standard numeric settings when optional parameters are invalid', () => {
        const details = parseTotpProvisioningUri('otpauth://totp/Veritas%3Aadmin%40example.com?digits=invalid&period=0')

        expect(details).toMatchObject({ digits: 6, periodSeconds: 30 })
    })

    it.each([null, 'not-a-uri', 'https://example.com/totp', 'otpauth://hotp/Veritas'])('rejects unsupported provisioning metadata: %s', (uri) => {
        expect(parseTotpProvisioningUri(uri)).toBeNull()
    })
})
