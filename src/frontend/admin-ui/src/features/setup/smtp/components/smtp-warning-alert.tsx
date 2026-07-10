import { Link } from '@tanstack/react-router'
import { AlertTriangle } from 'lucide-react'

import { Button } from '@/components/ui/button'

export function SmtpWarningAlert() {
    return (
        <div
            role="alert"
            className="flex flex-col gap-4 rounded-lg border border-amber-300 bg-amber-50 p-4 text-amber-950 sm:flex-row sm:items-center sm:justify-between"
        >
            <div className="flex items-start gap-3">
                <AlertTriangle className="mt-0.5 size-5 shrink-0" />
                <div>
                    <p className="font-medium">Email delivery is not configured</p>
                    <p className="mt-1 text-sm">Recovery, notifications, invitations, and email-dependent identity flows may be unavailable.</p>
                </div>
            </div>
            <Button asChild variant="outline" className="border-amber-400 bg-white hover:bg-amber-100">
                <Link to="/bootstrap/smtp">Configure SMTP</Link>
            </Button>
        </div>
    )
}
