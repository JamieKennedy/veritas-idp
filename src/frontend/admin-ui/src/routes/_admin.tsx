/* eslint-disable react-refresh/only-export-components -- TanStack Router route modules co-locate route definitions. */
import { Outlet, createFileRoute, useRouterState } from '@tanstack/react-router'

import { AdminShell } from '@/app/layouts/admin-shell'
import { requireAdminRoute } from '@/app/routing/guards'
import { SetupJourneyShell } from '@/features/setup/journey/components/setup-journey-shell'

export const Route = createFileRoute('/_admin')({
    ssr: false,
    beforeLoad: ({ context, location }) => requireAdminRoute(context.queryClient, location.pathname),
    component: AdminRoute,
})

function AdminRoute() {
    const { admin, setup } = Route.useRouteContext()
    const pathname = useRouterState({ select: (state) => state.location.pathname })

    if (pathname === '/bootstrap/smtp' && !setup.isSmtpConfigured) {
        return (
            <SetupJourneyShell currentStage="email">
                <Outlet />
            </SetupJourneyShell>
        )
    }

    return <AdminShell admin={admin} />
}
