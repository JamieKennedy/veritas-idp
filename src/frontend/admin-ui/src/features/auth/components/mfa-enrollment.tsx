import { useMutation } from '@tanstack/react-query'
import { useForm } from '@tanstack/react-form'
import { DownloadIcon } from 'lucide-react'
import { QRCodeCanvas } from 'qrcode.react'
import { useRef, useState } from 'react'

import { FieldError } from '@/app/forms/field-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getInlineRequestError } from '@/lib/api/request-errors'
import { ApiProblem } from '@/lib/api/http-client'
import { confirmMfaEnrollmentMutationOptions } from '../api/auth.api'
import { mfaEnrollmentSchema } from '../model/auth.schemas'
import { isLoginChallengeExpired, loginChallengeRestartNotice } from '../model/login-challenge'
import { parseTotpProvisioningUri } from '../model/totp-provisioning'
import type { LoginChallenge } from '../model/auth.schemas'

const qrCodeDownloadFilename = 'veritas-totp-qr.png'

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
    const qrCodeCanvasRef = useRef<HTMLCanvasElement>(null)
    const mutation = useMutation(confirmMfaEnrollmentMutationOptions())
    const provisioningDetails = parseTotpProvisioningUri(challenge.totpProvisioningUri)
    const advancedDetails = provisioningDetails
        ? [
              { label: 'Type', value: provisioningDetails.type },
              { label: 'Secret format', value: provisioningDetails.secretFormat },
              { label: 'Algorithm', value: provisioningDetails.algorithm },
              { label: 'Digits', value: provisioningDetails.digits.toString() },
              { label: 'Period', value: provisioningDetails.periodSeconds.toString() + ' seconds' },
              ...(provisioningDetails.issuer ? [{ label: 'Issuer', value: provisioningDetails.issuer }] : []),
              ...(provisioningDetails.account ? [{ label: 'Account', value: provisioningDetails.account }] : []),
          ]
        : []
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

    const downloadQrCode = () => {
        const canvas = qrCodeCanvasRef.current
        if (!canvas) {
            return
        }

        const download = document.createElement('a')
        download.download = qrCodeDownloadFilename
        download.href = canvas.toDataURL('image/png')
        download.click()
    }

    return (
        <div className="space-y-6">
            <div className="grid gap-5 sm:grid-cols-[10rem_1fr]">
                <div className="space-y-2">
                    <div className="flex items-center justify-center rounded-md border bg-white p-3">
                        {challenge.totpProvisioningUri && (
                            <QRCodeCanvas
                                ref={qrCodeCanvasRef}
                                value={challenge.totpProvisioningUri}
                                size={256}
                                marginSize={4}
                                title="Veritas TOTP enrollment QR code"
                                style={{ width: 128, height: 128 }}
                            />
                        )}
                    </div>
                    {challenge.totpProvisioningUri && (
                        <Button type="button" variant="outline" size="sm" className="w-full" onClick={downloadQrCode}>
                            <DownloadIcon />
                            Download QR code
                        </Button>
                    )}
                </div>
                <div>
                    <h2 className="font-medium">Enroll an authenticator</h2>
                    <p className="text-muted-foreground mt-2 text-sm">Scan the QR code, or enter this secret manually:</p>
                    <code className="bg-muted mt-3 block rounded-md border p-3 text-sm break-all">{challenge.totpSecretBase32}</code>
                    {advancedDetails.length > 0 && (
                        <details className="mt-3 rounded-md border">
                            <summary className="cursor-pointer px-3 py-2 text-sm font-medium">Advanced</summary>
                            <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-2 border-t p-3 text-sm">
                                {advancedDetails.map(({ label, value }) => (
                                    <div key={label} className="contents">
                                        <dt className="text-muted-foreground">{label}</dt>
                                        <dd className="text-right font-mono break-all">{value}</dd>
                                    </div>
                                ))}
                            </dl>
                        </details>
                    )}
                    {challenge.totpProvisioningUri && (
                        <p className="text-muted-foreground mt-3 text-xs">
                            The downloaded QR image contains your MFA secret. Store it securely and delete it after importing it.
                        </p>
                    )}
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
