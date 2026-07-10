import type { ZodType } from 'zod'
import { z } from 'zod'

import { adminApiBaseUrl } from '@/lib/env/env'

const csrfTokenSchema = z.object({ token: z.string().min(1) })
const apiProblemSchema = z.object({
    title: z.string().optional(),
    detail: z.string().optional(),
    status: z.number().int().optional(),
    errorCode: z.string().optional(),
    errorCategory: z.string().optional(),
})

interface ApiRequestOptions<T> {
    body?: unknown
    headers?: HeadersInit
    method?: 'DELETE' | 'GET' | 'PATCH' | 'POST' | 'PUT'
    schema?: ZodType<T>
    signal?: AbortSignal
}

let csrfTokenPromise: Promise<string> | undefined

export function resetCsrfToken() {
    csrfTokenPromise = undefined
}

export class ApiProblem extends Error {
    public readonly status: number
    public readonly errorCode?: string
    public readonly errorCategory?: string

    public constructor(status: number, message: string, errorCode?: string, errorCategory?: string) {
        super(message)
        this.name = 'ApiProblem'
        this.status = status
        this.errorCode = errorCode
        this.errorCategory = errorCategory
    }
}

export async function apiRequest<T = void>(path: string, options: ApiRequestOptions<T> = {}): Promise<T> {
    const method = options.method ?? 'GET'
    const headers = new Headers(options.headers)

    if (options.body !== undefined) {
        headers.set('Content-Type', 'application/json')
    }

    if (method !== 'GET') {
        headers.set('X-CSRF-TOKEN', await getCsrfToken())
    }

    const response = await fetch(`${adminApiBaseUrl}${path}`, {
        method,
        credentials: 'include',
        headers,
        body: options.body === undefined ? undefined : JSON.stringify(options.body),
        signal: options.signal,
    })

    if (!response.ok) {
        throw await toApiProblem(response)
    }

    if (response.status === 204 || response.headers.get('Content-Length') === '0') {
        return undefined as T
    }

    const text = await response.text()
    if (text.length === 0) {
        return undefined as T
    }

    const value: unknown = JSON.parse(text)
    return options.schema === undefined ? (value as T) : options.schema.parse(value)
}

async function getCsrfToken(): Promise<string> {
    csrfTokenPromise ??= fetch(`${adminApiBaseUrl}/api/v1/admin-auth/csrf`, {
        credentials: 'include',
    })
        .then(async (response) => {
            if (!response.ok) {
                throw await toApiProblem(response)
            }

            return csrfTokenSchema.parse(await response.json()).token
        })
        .catch((error: unknown) => {
            csrfTokenPromise = undefined
            throw error
        })

    return csrfTokenPromise
}

async function toApiProblem(response: Response): Promise<ApiProblem> {
    let parsed: z.infer<typeof apiProblemSchema> = {}

    try {
        parsed = apiProblemSchema.parse(await response.json())
    } catch {
        // Non-JSON failures are intentionally normalized below.
    }

    return new ApiProblem(response.status, parsed.detail ?? parsed.title ?? 'The request could not be completed.', parsed.errorCode, parsed.errorCategory)
}
