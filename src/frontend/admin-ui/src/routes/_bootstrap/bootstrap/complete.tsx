/* eslint-disable @typescript-eslint/only-throw-error -- TanStack Router redirects are control-flow objects. */
import { createFileRoute, redirect } from '@tanstack/react-router'

import { BootstrapCompletePage } from '@/features/setup/bootstrap/pages/bootstrap-complete-page'

export const Route = createFileRoute('/_bootstrap/bootstrap/complete')({
    beforeLoad: ({ context }) => {
        if (!context.setup.hasActiveBootstrap) {
            throw redirect({ to: '/bootstrap/start' })
        }
    },
    component: BootstrapCompletePage,
})
