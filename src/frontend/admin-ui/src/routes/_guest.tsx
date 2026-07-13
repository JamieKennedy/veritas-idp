/* eslint-disable react-refresh/only-export-components -- TanStack Router route modules co-locate route definitions. */
import { createFileRoute } from '@tanstack/react-router'

import { GuestShell } from '@/app/layouts/guest-shell'
import { requireGuestRoute } from '@/app/routing/guards'

export const Route = createFileRoute('/_guest')({
    ssr: false,
    beforeLoad: ({ context }) => requireGuestRoute(context.queryClient),
    component: GuestRoute,
})

function GuestRoute() {
    const { setup } = Route.useRouteContext()
    return <GuestShell isSetupJourney={!setup.isSmtpConfigured && !setup.isSmtpSetupDeferred} />
}
