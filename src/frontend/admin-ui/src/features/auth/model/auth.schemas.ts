import { z } from 'zod'

export const adminIdentitySchema = z.object({
    id: z.uuid(),
    email: z.email(),
    name: z.string().nullable(),
})

export type AdminIdentity = z.infer<typeof adminIdentitySchema>

export const loginChallengeSchema = z.object({
    challengeId: z.uuid(),
    challengeToken: z.string().min(1),
    purpose: z.enum(['MfaEnrollment', 'MfaVerification']),
    expiresAtUtc: z.iso.datetime({ offset: true }),
    totpSecretBase32: z.string().nullable(),
    totpProvisioningUri: z.string().nullable(),
})

export const mfaEnrollmentResponseSchema = z.object({
    admin: adminIdentitySchema,
    recoveryCodes: z.array(z.string().min(1)).min(1),
})

export const credentialsSchema = z.object({
    email: z.email('Enter a valid administrator email.'),
    password: z.string().min(1, 'Enter your password.'),
})

export const mfaEnrollmentSchema = z.object({
    code: z.string().regex(/^\d{6}$/, 'Enter the current 6-digit code.'),
})

export const mfaVerificationSchema = z.object({
    code: z.string().trim().min(1, 'Enter a TOTP or recovery code.'),
})

export type LoginChallenge = z.infer<typeof loginChallengeSchema>
export type CredentialsInput = z.infer<typeof credentialsSchema>
