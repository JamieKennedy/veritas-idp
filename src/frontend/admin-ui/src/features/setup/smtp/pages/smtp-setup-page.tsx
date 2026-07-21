import { useNavigate, useRouter } from '@tanstack/react-router'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { deferSmtpSetupMutationOptions } from '../../api/setup-status.api'
import { setupStatusQueryOptions } from '../../api/setup-status.query'
import { smtpSettingsQueryOptions } from '../api/smtp-settings.query'
import { SmtpForm } from '../components/smtp-form'
import { SmtpSkipDialog } from '../components/smtp-skip-dialog'

export function SmtpSetupPage() {
    const navigate = useNavigate()
    const router = useRouter()
    const queryClient = useQueryClient()
    const settings = useQuery(smtpSettingsQueryOptions())
    const deferSmtpSetup = useMutation(deferSmtpSetupMutationOptions())

    async function finish() {
        await Promise.all([
            queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey }),
            queryClient.invalidateQueries({ queryKey: smtpSettingsQueryOptions().queryKey }),
        ])
        await queryClient.fetchQuery(setupStatusQueryOptions())
        await router.invalidate()
        await navigate({ to: '/dashboard' })
    }

    async function skipSmtpSetup() {
        await deferSmtpSetup.mutateAsync()
        await queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey })
        await queryClient.fetchQuery(setupStatusQueryOptions())
        await router.invalidate()
        await navigate({ to: '/dashboard' })
    }

    return (
        <section className="setup-card setup-card--wide p-6 sm:p-8">
            <p className="setup-card__eyebrow">Installation setup</p>
            <h1 className="mt-2 text-2xl font-semibold">Configure email delivery</h1>
            <p className="text-muted-foreground mt-2 mb-7 text-sm">Veritas tests the connection before storing these settings.</p>
            {settings.isPending && <p className="text-muted-foreground text-sm">Loading SMTP settings...</p>}
            {settings.isError && (
                <p role="alert" className="text-destructive text-sm">
                    SMTP settings could not be loaded. Refresh to try again.
                </p>
            )}
            {settings.data && (
                <>
                    <SmtpForm
                        settings={settings.data}
                        onConfigured={() => {
                            void finish()
                        }}
                    />
                    <div className="mt-6 flex justify-end border-t pt-5">
                        <SmtpSkipDialog
                            isPending={deferSmtpSetup.isPending}
                            onSkip={() => {
                                void skipSmtpSetup()
                            }}
                        />
                    </div>
                </>
            )}
        </section>
    )
}
