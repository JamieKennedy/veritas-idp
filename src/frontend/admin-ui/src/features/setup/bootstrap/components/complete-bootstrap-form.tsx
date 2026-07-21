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
import { completeBootstrapMutationOptions } from '../api/bootstrap.api'
import { completeBootstrapSchema } from '../model/bootstrap.schemas'

export function CompleteBootstrapForm() {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [serverError, setServerError] = useState<string>()
    const mutation = useMutation(completeBootstrapMutationOptions())
    const form = useForm({
        defaultValues: {
            displayName: '',
            password: '',
            confirmPassword: '',
        },
        validators: {
            onSubmit: completeBootstrapSchema,
        },
        onSubmit: async ({ value }) => {
            setServerError(undefined)
            try {
                await mutation.mutateAsync(value)
                form.resetField('password')
                form.resetField('confirmPassword')
                await queryClient.invalidateQueries({ queryKey: setupStatusQueryOptions().queryKey })
                await queryClient.fetchQuery(setupStatusQueryOptions())
                await navigate({ to: '/login', search: { redirect: '/bootstrap/smtp' } })
            } catch (caught) {
                form.resetField('password')
                form.resetField('confirmPassword')
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
                        <form.Field name="displayName">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>
                                        Display name <span className="text-muted-foreground">(optional)</span>
                                    </Label>
                                    <Input
                                        id={field.name}
                                        name={field.name}
                                        autoComplete="name"
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
                                        autoComplete="new-password"
                                        aria-invalid={field.state.meta.errors.length > 0}
                                        aria-describedby={field.state.meta.errors.length > 0 ? `${field.name}-error` : undefined}
                                        value={field.state.value}
                                        onBlur={field.handleBlur}
                                        onChange={(event) => {
                                            field.handleChange(event.target.value)
                                        }}
                                    />
                                    <p className="text-muted-foreground text-xs">Use at least 12 characters.</p>
                                    <FieldError id={`${field.name}-error`} errors={field.state.meta.errors} />
                                </div>
                            )}
                        </form.Field>
                        <form.Field name="confirmPassword">
                            {(field) => (
                                <div className="space-y-2">
                                    <Label htmlFor={field.name}>Confirm password</Label>
                                    <Input
                                        id={field.name}
                                        name={field.name}
                                        type="password"
                                        autoComplete="new-password"
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
                            {isSubmitting ? 'Creating administrator...' : 'Create administrator'}
                        </Button>
                    </fieldset>
                )}
            </form.Subscribe>
        </form>
    )
}
