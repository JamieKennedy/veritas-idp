/* eslint-disable react-refresh/only-export-components -- TanStack Router route modules co-locate route definitions. */
import { createFileRoute } from '@tanstack/react-router'

import { AdminShell } from '@/app/layouts/admin-shell'
import { requireAdminRoute } from '@/app/routing/guards'

export const Route = createFileRoute('/_admin')({
    ssr: false,
    beforeLoad: ({ context, location }) => requireAdminRoute(context.queryClient, location.pathname),
    component: AdminRoute,
})

function AdminRoute() {
    const { admin } = Route.useRouteContext()
    return <AdminShell admin={admin} />
}
