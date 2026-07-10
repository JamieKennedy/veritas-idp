// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createMemoryHistory, createRouter, RouterProvider } from '@tanstack/react-router'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

describe('Admin UI root route', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.unstubAllGlobals()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
    })

    afterEach(() => {
        cleanup()
    })

    it('renders the dashboard SMTP warning for an authenticated partially configured installation', async () => {
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
                            isSmtpConfigured: false,
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
            history: createMemoryHistory({ initialEntries: ['/'] }),
            context: { queryClient },
        })
        await router.load()

        render(
            <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
            </QueryClientProvider>,
        )

        expect(await screen.findByRole('heading', { name: 'Dashboard' })).not.toBeNull()
        expect(screen.getByRole('alert').textContent).toContain('Email delivery is not configured')
    })

    it('routes a fresh installation to bootstrap start', async () => {
        vi.stubGlobal(
            'fetch',
            vi.fn((input: RequestInfo | URL) => {
                const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url
                if (url.endsWith('/api/v1/setup/status')) {
                    return Promise.resolve(
                        jsonResponse({
                            isConfigured: false,
                            hasActiveBootstrap: false,
                            activeBootstrapExpiresAtUtc: null,
                            isSmtpConfigured: false,
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
            history: createMemoryHistory({ initialEntries: ['/'] }),
            context: { queryClient },
        })
        await router.load()

        render(
            <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
            </QueryClientProvider>,
        )

        expect(await screen.findByRole('heading', { name: 'Claim this installation' })).not.toBeNull()
    })

    it('redirects bootstrap start to completion while a bootstrap session is active', async () => {
        vi.stubGlobal(
            'fetch',
            vi.fn((input: RequestInfo | URL) => {
                const url = typeof input === 'string' ? input : input instanceof URL ? input.href : input.url
                if (url.endsWith('/api/v1/setup/status')) {
                    return Promise.resolve(
                        jsonResponse({
                            isConfigured: false,
                            hasActiveBootstrap: true,
                            activeBootstrapExpiresAtUtc: '2026-07-10T11:00:00Z',
                            isSmtpConfigured: false,
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
            history: createMemoryHistory({ initialEntries: ['/bootstrap/start'] }),
            context: { queryClient },
        })
        await router.load()

        render(
            <QueryClientProvider client={queryClient}>
                <RouterProvider router={router} />
            </QueryClientProvider>,
        )

        expect(await screen.findByRole('heading', { name: 'Secure the first administrator' })).not.toBeNull()
    })
})

function jsonResponse(value: unknown) {
    return new Response(JSON.stringify(value), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
    })
}
