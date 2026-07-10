import { adminIdentitySchema, loginChallengeSchema, mfaEnrollmentResponseSchema } from '../model/auth.schemas'
import { ApiProblem, apiRequest, resetCsrfToken } from '@/lib/api/http-client'
import type { CredentialsInput, LoginChallenge } from '../model/auth.schemas'

export async function getCurrentAdmin(signal?: AbortSignal) {
    try {
        return await apiRequest('/api/v1/admin-auth/me', { schema: adminIdentitySchema, signal })
    } catch (error) {
        if (error instanceof ApiProblem && error.status === 401) {
            return null
        }

        throw error
    }
}

export function startAdminLogin(input: CredentialsInput) {
    return apiRequest('/api/v1/admin-auth/login', {
        method: 'POST',
        body: input,
        schema: loginChallengeSchema,
    })
}

export async function confirmMfaEnrollment(challenge: LoginChallenge, totpCode: string) {
    const response = await apiRequest('/api/v1/admin-auth/mfa/enroll/confirm', {
        method: 'POST',
        body: {
            challengeId: challenge.challengeId,
            challengeToken: challenge.challengeToken,
            totpCode,
        },
        schema: mfaEnrollmentResponseSchema,
    })
    resetCsrfToken()
    return response
}

export async function verifyMfa(challenge: LoginChallenge, code: string) {
    const response = await apiRequest('/api/v1/admin-auth/mfa/verify', {
        method: 'POST',
        body: {
            challengeId: challenge.challengeId,
            challengeToken: challenge.challengeToken,
            code,
        },
        schema: adminIdentitySchema,
    })
    resetCsrfToken()
    return response
}

export async function logoutAdmin() {
    await apiRequest('/api/v1/admin-auth/logout', { method: 'POST' })
    resetCsrfToken()
}
