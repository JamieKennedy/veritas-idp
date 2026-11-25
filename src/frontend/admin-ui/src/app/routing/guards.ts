/* eslint-disable @typescript-eslint/only-throw-error -- TanStack Router redirects are control-flow objects. */
import { redirect } from '@tanstack/react-router'
import type { QueryClient, QueryExecuteOptions, QueryKey } from '@tanstack/react-query'

import { currentAdminQueryOptions } from '@/features/auth/api/current-admin.query'
import { setupStatusQueryOptions } from '@/features/setup/api/setup-status.query'
import { resolveEntryDestination, sanitizeLoginRedirect } from './destinations'

export async function resolveRootRoute(queryClient: QueryClient) {
    const setup = await readCachedQuery(queryClient, setupStatusQueryOptions())
    const admin = setup.isConfigured ? await readCachedQuery(queryClient, currentAdminQueryOptions()) : null

    throw redirect({ to: resolveEntryDestination(setup, admin) })
}

export async function requireBootstrapRoute(queryClient: QueryClient) {
    const setup = await readCachedQuery(queryClient, setupStatusQueryOptions())
    if (!setup.isConfigured) {
        return { setup }
    }

    const admin = await readCachedQuery(queryClient, currentAdminQueryOptions())
    throw redirect({ to: resolveEntryDestination(setup, admin) })
}

export async function requireGuestRoute(queryClient: QueryClient) {
    const setup = await readCachedQuery(queryClient, setupStatusQueryOptions())
    if (!setup.isConfigured) {
        throw redirect({ to: resolveEntryDestination(setup, null) })
    }

    const admin = await readCachedQuery(queryClient, currentAdminQueryOptions())
    if (admin !== null) {
        throw redirect({ to: '/dashboard' })
    }

    return { setup }
}

export async function requireAdminRoute(queryClient: QueryClient, requestedPath: string) {
    const setup = await readCachedQuery(queryClient, setupStatusQueryOptions())
    if (!setup.isConfigured) {
        throw redirect({ to: resolveEntryDestination(setup, null) })
    }

    const admin = await readCachedQuery(queryClient, currentAdminQueryOptions())
    if (admin === null) {
        throw redirect({
            to: '/login',
            search: { redirect: sanitizeLoginRedirect(requestedPath) },
        })
    }

    return { admin, setup }
}

function readCachedQuery<TData, TKey extends QueryKey>(queryClient: QueryClient, options: QueryExecuteOptions<TData, Error, TData, TData, TKey>) {
    const cached = queryClient.getQueryData<TData>(options.queryKey)
    return cached === undefined ? queryClient.query(options) : Promise.resolve(cached)
}
