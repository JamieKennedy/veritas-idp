/* eslint-disable @typescript-eslint/only-throw-error -- TanStack Router redirects are control-flow objects. */
import { createFileRoute, redirect } from '@tanstack/react-router'

import { BootstrapStartPage } from '@/features/setup/bootstrap/pages/bootstrap-start-page'

export const Route = createFileRoute('/_bootstrap/bootstrap/start')({
    beforeLoad: ({ context }) => {
        if (context.setup.hasActiveBootstrap) {
            throw redirect({ to: '/bootstrap/complete' })
        }
    },
    component: BootstrapStartPage,
})
