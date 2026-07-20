// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createMemoryHistory, createRouter, RouterProvider } from '@tanstack/react-router'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

describe('Admin UI document metadata', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.unstubAllGlobals()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    afterEach(() => {
        cleanup()
    })

    it('publishes the shared Veritas browser icons', async () => {
        vi.stubGlobal(
            'fetch',
            vi.fn((input: RequestInfo | URL) => {
                const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url
                if (url.endsWith('/api/v1/setup/status')) {
                    return Promise.resolve(
                        jsonResponse({
                            isConfigured: true,
                            hasActiveBootstrap: false,
                            activeBootstrapExpiresAtUtc: null,
                            isSmtpConfigured: true,
                            isSmtpSetupDeferred: false,
                        }),
                    )
                }

                if (url.endsWith('/api/v1/admin-auth/me')) {
                    return Promise.resolve(
                        jsonResponse({
                            id: '5bc2e151-b36a-4698-a273-90ecf4b5aa7f',
                            email: 'admin@example.com',
                            name: 'First Admin',
                        }),
                    )
                }

                return Promise.resolve(new Response(null, { status: 404 }))
            }),
        )
        const { routeTree } = await import('@/routeTree.gen')
        const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
        const router = createRouter({
            routeTree,
            history: createMemoryHistory({ initialEntries: ['/dashboard'] }),
            context: { queryClient },
        })
        await router.load()

        render(
            <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
            </QueryClientProvider>,
        )

        expect(await screen.findByRole('heading', { name: 'Dashboard' })).not.toBeNull()
        expect(document.head.querySelector('link[rel="icon"][type="image/svg+xml"]')).not.toBeNull()
        expect(document.head.querySelector('link[rel="icon"][type="image/x-icon"]')).not.toBeNull()
        expect(document.head.querySelector('link[rel="apple-touch-icon"][sizes="180x180"][type="image/png"]')).not.toBeNull()
    })
})

function jsonResponse(value: unknown) {
    return new Response(JSON.stringify(value), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
    })
}
