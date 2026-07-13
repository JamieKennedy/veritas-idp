import veritasMarkUrl from '@brand/veritas-mark.svg?url'

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
                    <img src={veritasMarkUrl} alt="" className="setup-journey__mark" />
                    <span>Veritas</span>
                </div>
                <SetupProgress currentStage={currentStage} />
            </header>
            <main className="setup-journey__content">{children}</main>
        </div>
    )
}
