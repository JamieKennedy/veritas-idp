import { useMutation } from '@tanstack/react-query'
import { useForm } from '@tanstack/react-form'
import { useState } from 'react'

import { FieldError } from '@/app/forms/field-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getInlineRequestError } from '@/lib/api/request-errors'
import { startAdminLoginMutationOptions } from '../api/auth.api'
import { credentialsSchema } from '../model/auth.schemas'
import type { LoginChallenge } from '../model/auth.schemas'

export function CredentialsForm({ notice, onChallenge }: { notice?: string; onChallenge: (challenge: LoginChallenge) => void }) {
    const [serverError, setServerError] = useState<string>()
    const mutation = useMutation(startAdminLoginMutationOptions())
    const form = useForm({
        defaultValues: {
            email: '',
            password: '',
        },
        validators: {
            onSubmit: credentialsSchema,
        },
        onSubmit: async ({ value }) => {
            setServerError(undefined)
            try {
                const challenge = await mutation.mutateAsync(value)
                form.resetField('password')
                onChallenge(challenge)
            } catch (caught) {
                form.resetField('password')
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
                        {notice && !serverError && (
                            <p role="status" className="rounded-md border border-amber-300 bg-amber-50 p-3 text-sm text-amber-950">
                                {notice}
                            </p>
                        )}
                        <form.Field name="email">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>Administrator email</Label>
                                    <Input
                                        id={field.name}
                                        name={field.name}
                                        type="email"
                                        autoComplete="email"
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
                        <form.Field name="password">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>Password</Label>
                                    <Input
                                        id={field.name}
                                        name={field.name}
                                        type="password"
                                        autoComplete="current-password"
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
                        <Button className="w-full" type="submit" disabled={!canSubmit}>
                            {isSubmitting ? 'Checking credentials...' : 'Continue'}
                        </Button>
                    </fieldset>
                )}
            </form.Subscribe>
        </form>
    )
}
