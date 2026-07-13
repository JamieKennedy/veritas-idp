import { useMutation } from '@tanstack/react-query'
import { useForm } from '@tanstack/react-form'
import { useState } from 'react'

import { FieldError } from '@/app/forms/field-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getInlineRequestError } from '@/lib/api/request-errors'
import { ApiProblem } from '@/lib/api/http-client'
import { verifyMfaMutationOptions } from '../api/auth.api'
import { mfaVerificationSchema } from '../model/auth.schemas'
import { isLoginChallengeExpired, loginChallengeRestartNotice } from '../model/login-challenge'
import type { LoginChallenge } from '../model/auth.schemas'

export function MfaVerification({
    challenge,
    onComplete,
    onRejected,
    onReset,
}: {
    challenge: LoginChallenge
    onComplete: () => void
    onRejected: (notice: string) => void
    onReset: () => void
}) {
    const [serverError, setServerError] = useState<string>()
    const mutation = useMutation(verifyMfaMutationOptions())
    const form = useForm({
        defaultValues: {
            code: '',
        },
        validators: {
            onSubmit: mfaVerificationSchema,
        },
        onSubmit: async ({ value }) => {
            setServerError(undefined)
            if (isLoginChallengeExpired(challenge)) {
                form.resetField('code')
                onRejected(loginChallengeRestartNotice)
                return
            }

            try {
                await mutation.mutateAsync({ challenge, code: value.code.trim() })
                form.resetField('code')
                onComplete()
            } catch (caught) {
                form.resetField('code')
                if (caught instanceof ApiProblem && caught.status === 401) {
                    onRejected(loginChallengeRestartNotice)
                    return
                }

                setServerError(getInlineRequestError(caught))
            }
        },
    })

    return (
        <form
            onSubmit={(event) => {
                event.preventDefault()
                void form.handleSubmit()
            }}
        >
            <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
                {([canSubmit, isSubmitting]) => (
                    <fieldset className="space-y-5" disabled={isSubmitting}>
                        {serverError && (
                            <p role="alert" className="border-destructive/30 bg-destructive/5 text-destructive rounded-md border p-3 text-sm">
                                {serverError}
                            </p>
                        )}
                        <form.Field name="code">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>Authenticator or recovery code</Label>
                                    <Input
                                        id={field.name}
                                        name={field.name}
                                        autoComplete="one-time-code"
                                        aria-invalid={field.state.meta.errors.length > 0}
                                        aria-describedby={field.state.meta.errors.length > 0 ? `${field.name}-error` : undefined}
                                        value={field.state.value}
                                        onBlur={field.handleBlur}
                                        onChange={(event) => {
                                            field.handleChange(event.target.value)
                                        }}
                                    />
                                    <FieldError id={`${field.name}-error`} errors={field.state.meta.errors} />
                                </div>
                            )}
                        </form.Field>
                        <div className="flex gap-3">
                            <Button type="button" variant="outline" onClick={onReset}>
                                Start over
                            </Button>
                            <Button className="flex-1" type="submit" disabled={!canSubmit}>
                                {isSubmitting ? 'Verifying...' : 'Sign in'}
                            </Button>
                        </div>
                    </fieldset>
                )}
            </form.Subscribe>
        </form>
    )
}
