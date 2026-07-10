import { useNavigate } from '@tanstack/react-router'
import { useQueryClient } from '@tanstack/react-query'
import { useReducer } from 'react'

import { setupStatusQueryOptions } from '@/features/setup/api/setup-status.query'
import { currentAdminQueryOptions } from '../api/current-admin.query'
import { CredentialsForm } from '../components/credentials-form'
import { MfaEnrollment } from '../components/mfa-enrollment'
import { MfaVerification } from '../components/mfa-verification'
import { RecoveryCodes } from '../components/recovery-codes'
import { initialLoginFlowState, loginFlowReducer } from '../model/login-flow.reducer'
import type { LoginRedirect } from '@/app/routing/destinations'

export function LoginPage({ redirectTo }: { redirectTo: LoginRedirect }) {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [state, dispatch] = useReducer(loginFlowReducer, initialLoginFlowState)

    async function finishLogin() {
        await Promise.all([
            queryClient.invalidateQueries({ queryKey: currentAdminQueryOptions().queryKey }),
            queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey }),
        ])
        await navigate({ to: redirectTo })
    }

    return (
        <section className="bg-card rounded-lg border p-6 shadow-sm sm:p-8">
            <h1 className="text-2xl font-semibold">Administrator sign in</h1>
            <p className="text-muted-foreground mt-2 mb-7 text-sm">Password validation is followed by mandatory multi-factor authentication.</p>
            {state.step === 'credentials' && (
                <CredentialsForm
                    notice={state.notice}
                    onChallenge={(challenge) => {
                        dispatch({ type: 'challenge-started', challenge })
                    }}
                />
            )}
            {state.step === 'challenge' && state.challenge.purpose === 'MfaEnrollment' && (
                <MfaEnrollment
                    challenge={state.challenge}
                    onRejected={(notice) => {
                        dispatch({ type: 'challenge-rejected', notice })
                    }}
                    onReset={() => {
                        dispatch({ type: 'reset' })
                    }}
                    onComplete={(recoveryCodes) => {
                        dispatch({ type: 'enrollment-completed', recoveryCodes })
                    }}
                />
            )}
            {state.step === 'challenge' && state.challenge.purpose === 'MfaVerification' && (
                <MfaVerification
                    challenge={state.challenge}
                    onRejected={(notice) => {
                        dispatch({ type: 'challenge-rejected', notice })
                    }}
                    onReset={() => {
                        dispatch({ type: 'reset' })
                    }}
                    onComplete={() => {
                        void finishLogin()
                    }}
                />
            )}
            {state.step === 'recovery-codes' && (
                <RecoveryCodes
                    codes={state.recoveryCodes}
                    onContinue={() => {
                        void finishLogin()
                    }}
                />
            )}
        </section>
    )
}
