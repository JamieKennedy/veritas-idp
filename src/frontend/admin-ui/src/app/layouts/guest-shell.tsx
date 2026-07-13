import { Outlet } from '@tanstack/react-router'

import veritasLockupUrl from '@brand/veritas-lockup.svg?url'

import { ThemeControl } from '@/app/theme/theme-control'
import { SetupJourneyShell } from '@/features/setup/journey/components/setup-journey-shell'

export function GuestShell({ isSetupJourney }: { isSetupJourney: boolean }) {
    if (isSetupJourney) {
        return (
            <SetupJourneyShell currentStage="secure">
                <Outlet />
            </SetupJourneyShell>
        )
    }

    return (
        <main className="guest-shell">
            <div className="guest-shell__theme-control">
                <ThemeControl />
            </div>
            <div className="w-full max-w-md">
                <div className="guest-shell__brand mb-8">
                    <img src={veritasLockupUrl} alt="Veritas" className="guest-shell__lockup" />
                </div>
                <Outlet />
            </div>
        </main>
    )
}
