import { useMutation } from '@tanstack/react-query'
import { useForm } from '@tanstack/react-form'
import { useState } from 'react'

import { FieldError } from '@/app/forms/field-error'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { getInlineRequestError } from '@/lib/api/request-errors'
import { configureSmtpMutationOptions } from '../api/smtp.api'
import { configureSmtpSchema } from '../model/smtp.schemas'
import type { SmtpSettings } from '../model/smtp.schemas'

export function SmtpForm({ settings, onConfigured }: { settings: SmtpSettings; onConfigured: () => void }) {
    const [serverError, setServerError] = useState<string>()
    const mutation = useMutation(configureSmtpMutationOptions())
    const form = useForm({
        defaultValues: {
            host: settings.host,
            port: settings.port,
            tlsMode: settings.tlsMode,
            username: settings.username ?? '',
            secret: '',
            fromEmail: settings.fromEmail,
            fromName: settings.fromName ?? '',
        },
        validators: {
            onSubmit: configureSmtpSchema,
        },
        onSubmit: async ({ value }) => {
            setServerError(undefined)
            try {
                await mutation.mutateAsync(value)
                form.resetField('secret')
                onConfigured()
            } catch (caught) {
                form.resetField('secret')
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
                    <div className="space-y-6">
                        {serverError && (
                            <p role="alert" className="border-destructive/30 bg-destructive/5 text-destructive rounded-md border p-3 text-sm">
                                {serverError}
                            </p>
                        )}
                        <fieldset className="grid gap-4 sm:grid-cols-[1fr_8rem]" disabled={isSubmitting}>
                            <legend className="mb-3 font-medium sm:col-span-2">Connection</legend>
                            <form.Field name="host">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>Host</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.value)
                                            }}
                                        />
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                            <form.Field name="port">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>Port</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            type="number"
                                            min={1}
                                            max={65535}
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.valueAsNumber)
                                            }}
                                        />
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                            <form.Field name="tlsMode">
                                {(field) => (
                                    <div className="space-y-2 sm:col-span-2">
                                        <Label htmlFor={field.name}>Transport security</Label>
                                        <Select
                                            value={field.state.value}
                                            onValueChange={(value) => {
                                                field.handleChange(value as 'None' | 'StartTls')
                                            }}
                                        >
                                            <SelectTrigger id={field.name} className="w-full" onBlur={field.handleBlur}>
                                                <SelectValue />
                                            </SelectTrigger>
                                            <SelectContent>
                                                <SelectItem value="StartTls">STARTTLS</SelectItem>
                                                <SelectItem value="None">None (development only)</SelectItem>
                                            </SelectContent>
                                        </Select>
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                        </fieldset>
                        <fieldset className="grid gap-4 sm:grid-cols-2" disabled={isSubmitting}>
                            <legend className="mb-3 font-medium sm:col-span-2">Authentication</legend>
                            <form.Field name="username">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>Username</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            autoComplete="username"
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.value)
                                            }}
                                        />
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                            <form.Field name="secret">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>Password or secret</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            type="password"
                                            autoComplete="new-password"
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.value)
                                            }}
                                        />
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                        </fieldset>
                        <fieldset className="grid gap-4 sm:grid-cols-2" disabled={isSubmitting}>
                            <legend className="mb-3 font-medium sm:col-span-2">Sender identity</legend>
                            <form.Field name="fromEmail">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>From email</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            type="email"
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.value)
                                            }}
                                        />
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                            <form.Field name="fromName">
                                {(field) => (
                                    <div className="space-y-2">
                                        <Label htmlFor={field.name}>From name</Label>
                                        <Input
                                            id={field.name}
                                            name={field.name}
                                            value={field.state.value}
                                            onBlur={field.handleBlur}
                                            onChange={(event) => {
                                                field.handleChange(event.target.value)
                                            }}
                                        />
                                        <FieldError errors={field.state.meta.errors} />
                                    </div>
                                )}
                            </form.Field>
                        </fieldset>
                        <Button className="w-full" type="submit" disabled={!canSubmit || isSubmitting}>
                            {isSubmitting ? 'Testing connection...' : 'Save and test connection'}
                        </Button>
                    </div>
                )}
            </form.Subscribe>
        </form>
    )
}
