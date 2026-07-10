import { createFileRoute } from '@tanstack/react-router'

import { GuestShell } from '@/app/layouts/guest-shell'
import { requireGuestRoute } from '@/app/routing/guards'

export const Route = createFileRoute('/_guest')({
    ssr: false,
    beforeLoad: ({ context }) => requireGuestRoute(context.queryClient),
    component: GuestShell,
})
