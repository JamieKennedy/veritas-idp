// @vitest-environment jsdom

import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createMemoryHistory, createRouter, RouterProvider } from '@tanstack/react-router'
import { cleanup, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

describe('Admin UI theme initialization', () => {
    beforeEach(() => {
        vi.resetModules()
        vi.unstubAllEnvs()
        vi.unstubAllGlobals()
        vi.stubEnv('VITE_ADMIN_API_BASE_URL', 'https://localhost:7100')
        vi.stubGlobal('localStorage', { getItem: () => null, setItem: vi.fn() })
        vi.stubGlobal(
            'matchMedia',
            vi.fn().mockReturnValue({
                matches: true,
                addEventListener: vi.fn(),
                removeEventListener: vi.fn(),
            }),
        )
        document.documentElement.className = ''
        document.documentElement.style.colorScheme = ''
    })

    afterEach(() => {
        cleanup()
    })

    it('applies the system dark theme to a routed admin screen', async () => {
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
        expect(document.documentElement.classList.contains('dark')).toBe(true)
        expect(document.documentElement.style.colorScheme).toBe('dark')
    })
})

function jsonResponse(value: unknown) {
    return new Response(JSON.stringify(value), {
        status: 200,
        headers: { 'Content-Type': 'application/json' },
    })
}
