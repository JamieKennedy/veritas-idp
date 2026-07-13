import { Outlet } from '@tanstack/react-router'

import veritasMarkUrl from '@brand/veritas-mark.svg?url'

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
                <div className="mb-8 flex items-center justify-center gap-3 text-xl font-semibold">
                    <img src={veritasMarkUrl} alt="" className="size-8" />
                    Veritas
                </div>
                <Outlet />
            </div>
        </main>
    )
}
