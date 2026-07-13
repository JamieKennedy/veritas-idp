import { useMutation } from '@tanstack/react-query'
import { useForm } from '@tanstack/react-form'
import { QRCodeSVG } from 'qrcode.react'
import { useState } from 'react'

import { FieldError } from '@/app/forms/field-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getInlineRequestError } from '@/lib/api/request-errors'
import { ApiProblem } from '@/lib/api/http-client'
import { confirmMfaEnrollmentMutationOptions } from '../api/auth.api'
import { mfaEnrollmentSchema } from '../model/auth.schemas'
import { isLoginChallengeExpired, loginChallengeRestartNotice } from '../model/login-challenge'
import type { LoginChallenge } from '../model/auth.schemas'

export function MfaEnrollment({
    challenge,
    onComplete,
    onRejected,
    onReset,
}: {
    challenge: LoginChallenge
    onComplete: (recoveryCodes: ReadonlyArray<string>) => void
    onRejected: (notice: string) => void
    onReset: () => void
}) {
    const [serverError, setServerError] = useState<string>()
    const mutation = useMutation(confirmMfaEnrollmentMutationOptions())
    const form = useForm({
        defaultValues: {
            code: '',
        },
        validators: {
            onSubmit: mfaEnrollmentSchema,
        },
        onSubmit: async ({ value }) => {
            setServerError(undefined)
            if (isLoginChallengeExpired(challenge)) {
                form.resetField('code')
                onRejected(loginChallengeRestartNotice)
                return
            }

            try {
                const result = await mutation.mutateAsync({ challenge, totpCode: value.code })
                form.resetField('code')
                onComplete(result.recoveryCodes)
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
        <div className="space-y-6">
            <div className="grid gap-5 sm:grid-cols-[10rem_1fr]">
                <div className="flex items-center justify-center rounded-md border bg-white p-3">
                    {challenge.totpProvisioningUri && <QRCodeSVG value={challenge.totpProvisioningUri} size={128} />}
                </div>
                <div>
                    <h2 className="font-medium">Enroll an authenticator</h2>
                    <p className="text-muted-foreground mt-2 text-sm">Scan the QR code, or enter this secret manually:</p>
                    <code className="bg-muted mt-3 block rounded-md border p-3 text-sm break-all">{challenge.totpSecretBase32}</code>
                </div>
            </div>
            <form
                onSubmit={(event) => {
                    event.preventDefault()
                    void form.handleSubmit()
                }}
            >
                <form.Subscribe selector={(state) => [state.canSubmit, state.isSubmitting] as const}>
                    {([canSubmit, isSubmitting]) => (
                        <fieldset className="space-y-4" disabled={isSubmitting}>
                            {serverError && (
                                <p role="alert" className="text-destructive text-sm">
                                    {serverError}
                                </p>
                            )}
                            <form.Field name="code">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>Verification code</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            inputMode="numeric"
                                            autoComplete="one-time-code"
                                            maxLength={6}
                                            aria-invalid={field.state.meta.errors.length > 0}
                                            aria-describedby={field.state.meta.errors.length > 0 ? `${field.name}-error` : undefined}
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.value.replace(/\D/g, ''))
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
                                    {isSubmitting ? 'Verifying...' : 'Verify and enroll'}
                                </Button>
                            </div>
                        </fieldset>
                    )}
                </form.Subscribe>
            </form>
        </div>
    )
}
