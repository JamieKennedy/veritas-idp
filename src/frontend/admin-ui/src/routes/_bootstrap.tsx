import { createFileRoute } from '@tanstack/react-router'

import { BootstrapShell } from '@/app/layouts/bootstrap-shell'
import { requireBootstrapRoute } from '@/app/routing/guards'

export const Route = createFileRoute('/_bootstrap')({
    ssr: false,
    beforeLoad: ({ context }) => requireBootstrapRoute(context.queryClient),
    component: BootstrapShell,
})
