interface FieldErrorProps {
    errors: ReadonlyArray<unknown>
}

export function FieldError({ errors }: FieldErrorProps) {
    const message = getErrorMessage(errors[0])

    if (message === undefined) {
        return null
    }

    return <p className="text-destructive text-xs">{message}</p>
}

function getErrorMessage(error: unknown) {
    if (typeof error === 'string') {
        return error
    }

    if (typeof error === 'object' && error !== null && 'message' in error && typeof error.message === 'string') {
        return error.message
    }

    return undefined
}
