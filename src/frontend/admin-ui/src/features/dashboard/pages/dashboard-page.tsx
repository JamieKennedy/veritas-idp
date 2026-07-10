import { useQuery } from '@tanstack/react-query'

import { setupStatusQueryOptions } from '@/features/setup/api/setup-status.query'
import { SmtpWarningAlert } from '@/features/setup/smtp'
import { ReadinessList } from '../components/readiness-list'
import { RecentActivity } from '../components/recent-activity'

export function DashboardPage() {
    const setup = useQuery(setupStatusQueryOptions())

    if (setup.isPending) {
        return <p className="text-muted-foreground text-sm">Loading dashboard...</p>
    }

    if (setup.isError) {
        return (
            <p role="alert" className="text-destructive text-sm">
                Setup status could not be loaded. Refresh to try again.
            </p>
        )
    }

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-semibold">Dashboard</h1>
                <p className="text-muted-foreground mt-1 text-sm">Installation readiness and administrative activity.</p>
            </div>
            {!setup.data.isSmtpConfigured && <SmtpWarningAlert />}
            <div className="grid gap-6 xl:grid-cols-2">
                <ReadinessList isSmtpConfigured={setup.data.isSmtpConfigured} />
                <RecentActivity />
            </div>
        </div>
    )
}
