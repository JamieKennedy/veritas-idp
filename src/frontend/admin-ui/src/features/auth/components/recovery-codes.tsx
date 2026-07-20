import { useForm } from '@tanstack/react-form'

import { Button } from '@/components/ui/button'

export function RecoveryCodes({ codes, onContinue }: { codes: ReadonlyArray<string>; onContinue: () => void }) {
    const form = useForm({
        defaultValues: {
            acknowledged: false,
        },
        onSubmit: ({ value }) => {
            if (value.acknowledged) {
                onContinue()
            }
        },
    })

    return (
        <form
            className="space-y-5"
            onSubmit={(event) => {
                event.preventDefault()
                void form.handleSubmit()
            }}
        >
            <p className="rounded-md border border-amber-300 bg-amber-50 p-3 text-sm text-amber-950">
                These recovery codes are shown once. Store them somewhere secure.
            </p>
            <ul className="bg-muted grid gap-2 rounded-md border p-4 font-mono text-sm sm:grid-cols-2">
                {codes.map((code) => (
                    <li key={code}>{code}</li>
                ))}
            </ul>
            <form.Field name="acknowledged">
                {(field) => (
                    <label className="flex items-start gap-3 text-sm">
                        <input
                            className="mt-1 size-4"
                            type="checkbox"
                            name={field.name}
                            checked={field.state.value}
                            onBlur={field.handleBlur}
                            onChange={(event) => {
                                field.handleChange(event.target.checked)
                            }}
                        />
                        I have stored these recovery codes securely.
                    </label>
                )}
            </form.Field>
            <form.Subscribe selector={(state) => state.values.acknowledged}>
                {(acknowledged) => (
                    <Button className="w-full" type="submit" disabled={!acknowledged}>
                        Continue to Veritas
                    </Button>
                )}
            </form.Subscribe>
        </form>
    )
}
