import type { LoginChallenge } from './auth.schemas'

export type LoginFlowState =
    | { step: 'credentials'; notice?: string }
    | { step: 'challenge'; challenge: LoginChallenge }
    | { step: 'recovery-codes'; recoveryCodes: ReadonlyArray<string> }

export type LoginFlowAction =
    | { type: 'challenge-started'; challenge: LoginChallenge }
    | { type: 'challenge-rejected'; notice: string }
    | { type: 'enrollment-completed'; recoveryCodes: ReadonlyArray<string> }
    | { type: 'reset' }

export const initialLoginFlowState: LoginFlowState = { step: 'credentials' }

export function loginFlowReducer(_state: LoginFlowState, action: LoginFlowAction): LoginFlowState {
    switch (action.type) {
        case 'challenge-started':
            return { step: 'challenge', challenge: action.challenge }
        case 'challenge-rejected':
            return { step: 'credentials', notice: action.notice }
        case 'enrollment-completed':
            return { step: 'recovery-codes', recoveryCodes: action.recoveryCodes }
        case 'reset':
            return initialLoginFlowState
    }
}
