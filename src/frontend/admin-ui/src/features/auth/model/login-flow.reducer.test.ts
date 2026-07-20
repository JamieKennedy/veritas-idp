import { describe, expect, it } from 'vitest'

import { initialLoginFlowState, loginFlowReducer } from './login-flow.reducer'

const challenge = {
    challengeId: '5bc2e151-b36a-4698-a273-90ecf4b5aa7f',
    challengeToken: 'one-time-token',
    purpose: 'MfaEnrollment' as const,
    expiresAtUtc: '2026-07-10T11:00:00Z',
    totpSecretBase32: 'JBSWY3DPEHPK3PXP',
    totpProvisioningUri: 'otpauth://totp/Veritas',
}

describe('loginFlowReducer', () => {
    it('moves from credentials to an MFA challenge', () => {
        expect(loginFlowReducer(initialLoginFlowState, { type: 'challenge-started', challenge })).toEqual({
            step: 'challenge',
            challenge,
        })
    })

    it('moves enrollment completion to one-time recovery codes', () => {
        const state = loginFlowReducer(
            { step: 'challenge', challenge },
            {
                type: 'enrollment-completed',
                recoveryCodes: ['recovery-1'],
            },
        )

        expect(state).toEqual({ step: 'recovery-codes', recoveryCodes: ['recovery-1'] })
    })

    it('resets challenge material to the credential step', () => {
        expect(loginFlowReducer({ step: 'challenge', challenge }, { type: 'reset' })).toBe(initialLoginFlowState)
    })

    it('returns a rejected challenge to credentials with a safe notice', () => {
        expect(loginFlowReducer({ step: 'challenge', challenge }, { type: 'challenge-rejected', notice: 'The challenge must be restarted.' })).toEqual({
            step: 'credentials',
            notice: 'The challenge must be restarted.',
        })
    })
})
