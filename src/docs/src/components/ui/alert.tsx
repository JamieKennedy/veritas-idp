import type * as React from 'react'
import { cva } from 'class-variance-authority'
import type { VariantProps } from 'class-variance-authority'

import { cn } from '@/lib/utils'

const alertVariants = cva('relative grid w-full grid-cols-[auto_1fr] gap-x-3 rounded-xl border px-4 py-3 text-sm', {
    variants: {
        variant: {
            default: 'border-border bg-card text-card-foreground',
            warning: 'border-warning/35 bg-warning/8 text-foreground',
        },
    },
    defaultVariants: {
        variant: 'default',
    },
})

function Alert({ className, variant, ...props }: React.ComponentProps<'div'> & VariantProps<typeof alertVariants>) {
    return <div role="alert" data-slot="alert" className={cn(alertVariants({ variant }), className)} {...props} />
}

function AlertTitle({ className, ...props }: React.ComponentProps<'div'>) {
    return <div data-slot="alert-title" className={cn('col-start-2 font-semibold', className)} {...props} />
}

function AlertDescription({ className, ...props }: React.ComponentProps<'div'>) {
    return <div data-slot="alert-description" className={cn('col-start-2 mt-0.5 leading-relaxed text-muted-foreground', className)} {...props} />
}

export { Alert, AlertTitle, AlertDescription }
