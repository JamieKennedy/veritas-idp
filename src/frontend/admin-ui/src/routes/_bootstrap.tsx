/* eslint-disable react-refresh/only-export-components -- TanStack Router route modules co-locate route definitions. */
import { createFileRoute } from '@tanstack/react-router'

import { BootstrapShell } from '@/app/layouts/bootstrap-shell'
import { requireBootstrapRoute } from '@/app/routing/guards'

export const Route = createFileRoute('/_bootstrap')({
    ssr: false,
    beforeLoad: ({ context }) => requireBootstrapRoute(context.queryClient),
    component: BootstrapRoute,
})

function BootstrapRoute() {
    const { setup } = Route.useRouteContext()
    return <BootstrapShell currentStage={setup.hasActiveBootstrap ? 'secure' : 'claim'} />
}
