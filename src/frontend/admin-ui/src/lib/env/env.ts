import { z } from 'zod'

const adminUiEnvSchema = z.object({
    VITE_ADMIN_API_BASE_URL: z.union([z.literal(''), z.url()]).optional(),
})

export interface AdminUiRuntimeConfig {
    readonly adminApiBaseUrl: string
}

export function parseAdminUiEnv(env: ImportMetaEnv): AdminUiRuntimeConfig {
    const parsed = adminUiEnvSchema.parse(env)

    return {
        adminApiBaseUrl: parsed.VITE_ADMIN_API_BASE_URL?.replace(/\/$/, '') ?? '',
    }
}

export const adminApiBaseUrl = parseAdminUiEnv(import.meta.env).adminApiBaseUrl
