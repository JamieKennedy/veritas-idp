import veritasLockupUrl from '@brand/veritas-lockup.svg?url'

import { ThemeControl } from '@/app/theme/theme-control'
import { SetupProgress } from './setup-progress'
import type { SetupStage } from '../model/setup-stage'

export function SetupJourneyShell({ children, currentStage }: { children: React.ReactNode; currentStage: SetupStage }) {
    return (
        <div className="setup-journey">
            <header className="setup-journey__header">
                <div className="setup-journey__theme-control">
                    <ThemeControl />
                </div>
                <div className="setup-journey__brand">
                    <img src={veritasLockupUrl} alt="Veritas" className="setup-journey__lockup" />
                </div>
                <SetupProgress currentStage={currentStage} />
            </header>
            <main className="setup-journey__content">{children}</main>
        </div>
    )
}
