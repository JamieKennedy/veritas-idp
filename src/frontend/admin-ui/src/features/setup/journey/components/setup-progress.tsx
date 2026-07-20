import { Check } from 'lucide-react'

import { setupStages } from '../model/setup-stage'
import type { SetupStage } from '../model/setup-stage'

export function SetupProgress({ currentStage }: { currentStage: SetupStage }) {
    const activeIndex = setupStages.findIndex((stage) => stage.id === currentStage)

    return (
        <ol className="setup-progress" aria-label="Installation setup progress">
            {setupStages.map((stage, index) => {
                const isComplete = index < activeIndex
                const isCurrent = index === activeIndex

                return (
                    <li key={stage.id} className="setup-progress__stage" aria-current={isCurrent ? 'step' : undefined}>
                        <span
                            className={['setup-progress__circle', isComplete && 'is-complete', isCurrent && 'is-current'].filter(Boolean).join(' ')}
                            aria-hidden="true"
                        >
                            {isComplete ? <Check className="size-4" strokeWidth={3} /> : index + 1}
                        </span>
                        <span className="setup-progress__label">{stage.label}</span>
                        <span className="sr-only">
                            {isComplete ? `Complete: ${stage.label}` : isCurrent ? `Current step: ${stage.label}` : `Upcoming: ${stage.label}`}
                        </span>
                        {index < setupStages.length - 1 && (
                            <span className={['setup-progress__connector', isComplete && 'is-complete'].filter(Boolean).join(' ')} aria-hidden="true" />
                        )}
                    </li>
                )
            })}
        </ol>
    )
}
