import type * as React from 'react'
import { cva } from 'class-variance-authority'
import type { VariantProps } from 'class-variance-authority'

import { cn } from '@/lib/utils'

const badgeVariants = cva('inline-flex items-center rounded-full border px-2.5 py-1 text-[0.6875rem] leading-none font-semibold tracking-[0.08em] uppercase', {
    variants: {
        variant: {
            default: 'border-primary/25 bg-primary/10 text-primary',
            outline: 'border-border bg-background/70 text-muted-foreground',
            warning: 'border-warning/30 bg-warning/10 text-warning-foreground',
        },
    },
    defaultVariants: {
        variant: 'default',
    },
})

function Badge({ className, variant, ...props }: React.ComponentProps<'span'> & VariantProps<typeof badgeVariants>) {
    return <span data-slot="badge" className={cn(badgeVariants({ variant }), className)} {...props} />
}

export { Badge }
