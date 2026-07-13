import { BrandLockup } from '@/app/brand/brand-lockup'
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
                    <BrandLockup className="setup-journey__lockup" />
                </div>
                <SetupProgress currentStage={currentStage} />
            </header>
            <main className="setup-journey__content">{children}</main>
        </div>
    )
}
