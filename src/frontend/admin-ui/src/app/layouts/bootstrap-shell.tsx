import { Outlet } from '@tanstack/react-router'

import { SetupJourneyShell } from '@/features/setup/journey/components/setup-journey-shell'
import type { SetupStage } from '@/features/setup/journey/model/setup-stage'

export function BootstrapShell({ currentStage }: { currentStage: SetupStage }) {
    return (
        <SetupJourneyShell currentStage={currentStage}>
            <Outlet />
        </SetupJourneyShell>
    )
}
