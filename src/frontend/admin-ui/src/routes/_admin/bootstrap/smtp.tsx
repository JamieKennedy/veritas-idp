/* eslint-disable @typescript-eslint/only-throw-error -- TanStack Router redirects are control-flow objects. */
import { createFileRoute, redirect } from '@tanstack/react-router'

import { SmtpSetupPage } from '@/features/setup/smtp/pages/smtp-setup-page'

export const Route = createFileRoute('/_admin/bootstrap/smtp')({
    beforeLoad: ({ context }) => {
        if (context.setup.isSmtpConfigured) {
            throw redirect({ to: '/dashboard' })
        }
    },
    component: SmtpSetupPage,
})
