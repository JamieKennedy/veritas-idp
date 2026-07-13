import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useForm } from '@tanstack/react-form'
import { useNavigate } from '@tanstack/react-router'
import { useState } from 'react'

import { FieldError } from '@/app/forms/field-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getInlineRequestError } from '@/lib/api/request-errors'
import { setupStatusQueryOptions } from '../../api/setup-status.query'
import { startBootstrapMutationOptions } from '../api/bootstrap.api'
import { startBootstrapSchema } from '../model/bootstrap.schemas'

export function StartBootstrapForm() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [serverError, setServerError] = useState<string>()
    const mutation = useMutation(startBootstrapMutationOptions())
    const form = useForm({
        defaultValues: {
            email: '',
            bootstrapSecret: '',
        },
        validators: {
            onSubmit: startBootstrapSchema,
        },
        onSubmit: async ({ value }) => {
            setServerError(undefined)
            try {
                await mutation.mutateAsync(value)
                form.resetField('bootstrapSecret')
                await queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey })
                await navigate({ to: '/bootstrap/complete' })
            } catch (caught) {
                form.resetField('bootstrapSecret')
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
                        <form.Field name="email">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>First administrator email</Label>
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
                        <form.Field name="bootstrapSecret">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>Deployment bootstrap secret</Label>
                                    <Input
                                        id={field.name}
                                        name={field.name}
                                        type="password"
                                        autoComplete="off"
                                        aria-invalid={field.state.meta.errors.length > 0}
                                        aria-describedby={field.state.meta.errors.length > 0 ? `${field.name}-error` : undefined}
                                        value={field.state.value}
                                        onBlur={field.handleBlur}
                                        onChange={(event) => {
                                            field.handleChange(event.target.value)
                                        }}
                                    />
                                    <p className="text-muted-foreground text-xs">The secret is submitted once and is not stored by this UI.</p>
                                    <FieldError id={`${field.name}-error`} errors={field.state.meta.errors} />
                                </div>
                            )}
                        </form.Field>
                        <Button className="w-full" type="submit" disabled={!canSubmit}>
                            {isSubmitting ? 'Verifying...' : 'Verify and continue'}
                        </Button>
                    </fieldset>
                )}
            </form.Subscribe>
        </form>
    )
}
