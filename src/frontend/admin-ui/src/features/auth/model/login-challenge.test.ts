import { describe, expect, it } from 'vitest'

import { isLoginChallengeExpired } from './login-challenge'
import type { LoginChallenge } from './auth.schemas'

const challenge: LoginChallenge = {
    challengeId: 'd34ec287-68ef-4ddc-861b-edc2f334bf84',
    challengeToken: 'short-lived-token',
    purpose: 'MfaVerification',
    expiresAtUtc: '2026-07-10T11:00:00Z',
    totpSecretBase32: null,
    totpProvisioningUri: null,
}

describe('isLoginChallengeExpired', () => {
    it('treats the expiry instant as expired', () => {
        expect(isLoginChallengeExpired(challenge, Date.parse('2026-07-10T11:00:00Z'))).toBe(true)
    })

    it('keeps a future challenge active', () => {
        expect(isLoginChallengeExpired(challenge, Date.parse('2026-07-10T10:59:59Z'))).toBe(false)
    })
})
