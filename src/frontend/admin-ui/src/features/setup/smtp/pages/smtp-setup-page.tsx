import { useNavigate } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { deferSmtpSetupMutationOptions } from '../../api/setup-status.api'
import { setupStatusQueryOptions } from '../../api/setup-status.query'
import { smtpSettingsQueryOptions } from '../api/smtp-settings.query'
import { SmtpForm } from '../components/smtp-form'
import { SmtpSkipDialog } from '../components/smtp-skip-dialog'

export function SmtpSetupPage() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const settings = useQuery(smtpSettingsQueryOptions())
    const deferSmtpSetup = useMutation(deferSmtpSetupMutationOptions())

    async function finish() {
        await Promise.all([
            queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey }),
            queryClient.invalidateQueries({ queryKey: smtpSettingsQueryOptions().queryKey }),
        ])
        await navigate({ to: '/dashboard' })
    }

    async function skipSmtpSetup() {
        await deferSmtpSetup.mutateAsync()
        await queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey })
        await navigate({ to: '/dashboard' })
    }

    if (settings.isPending) {
        return <p className="text-muted-foreground text-sm">Loading SMTP settings...</p>
    }

    if (settings.isError) {
        return (
            <p role="alert" className="text-destructive text-sm">
                SMTP settings could not be loaded. Refresh to try again.
            </p>
        )
    }

    return (
        <div className="mx-auto max-w-3xl space-y-6">
            <div>
                <p className="text-sm font-medium text-[#328f97]">Step 3 of 3</p>
                <h1 className="mt-2 text-2xl font-semibold">Configure email delivery</h1>
                <p className="text-muted-foreground mt-2 text-sm">Veritas tests the connection before storing these settings.</p>
            </div>
            <section className="bg-card rounded-lg border p-6 shadow-sm">
                <SmtpForm
                    settings={settings.data}
                    onConfigured={() => {
                        void finish()
                    }}
                />
            </section>
            <div className="flex justify-end">
                <SmtpSkipDialog
                    isPending={deferSmtpSetup.isPending}
                    onSkip={() => {
                        void skipSmtpSetup()
                    }}
                />
            </div>
        </div>
    )
}
