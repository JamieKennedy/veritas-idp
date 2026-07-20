import { z } from 'zod'

export const startBootstrapSchema = z.object({
    email: z.email('Enter a valid administrator email.'),
    bootstrapSecret: z.string().trim().min(1, 'Enter the deployment bootstrap secret.'),
})

export const completeBootstrapSchema = z
    .object({
        displayName: z.string().trim().max(200),
        password: z.string().min(12, 'Use at least 12 characters.'),
        confirmPassword: z.string(),
    })
    .refine((value) => value.password === value.confirmPassword, {
        message: 'Passwords do not match.',
        path: ['confirmPassword'],
    })

export type StartBootstrapInput = z.infer<typeof startBootstrapSchema>
export type CompleteBootstrapInput = z.infer<typeof completeBootstrapSchema>
