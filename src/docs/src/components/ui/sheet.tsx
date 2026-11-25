import type * as React from 'react'
import { X } from 'lucide-react'
import { Dialog as SheetPrimitive } from 'radix-ui'

import { cn } from '@/lib/utils'

const Sheet = SheetPrimitive.Root
const SheetTrigger = SheetPrimitive.Trigger
const SheetClose = SheetPrimitive.Close

function SheetContent({
    className,
    children,
    side = 'right',
    ...props
}: React.ComponentProps<typeof SheetPrimitive.Content> & {
    side?: 'left' | 'right'
}) {
    return (
        <SheetPrimitive.Portal>
            <SheetPrimitive.Overlay className="fixed inset-0 z-50 bg-slate-950/65 backdrop-blur-sm" />
            <SheetPrimitive.Content
                data-slot="sheet-content"
                data-side={side}
                className={cn(
                    'fixed inset-y-0 z-50 flex w-[min(22rem,90vw)] flex-col border bg-background shadow-2xl outline-none',
                    side === 'left' ? 'left-0 border-r' : 'right-0 border-l',
                    className,
                )}
                {...props}
            >
                {children}
                <SheetPrimitive.Close className="absolute top-4 right-4 rounded-md p-1 text-muted-foreground outline-none hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring">
                    <X className="size-4" />
                    <span className="sr-only">Close navigation</span>
                </SheetPrimitive.Close>
            </SheetPrimitive.Content>
        </SheetPrimitive.Portal>
    )
}

function SheetHeader({ className, ...props }: React.ComponentProps<'div'>) {
    return <div className={cn('border-b px-5 py-4 pe-12', className)} {...props} />
}

function SheetTitle({ className, ...props }: React.ComponentProps<typeof SheetPrimitive.Title>) {
    return <SheetPrimitive.Title className={cn('font-heading text-lg font-semibold', className)} {...props} />
}

function SheetDescription({ className, ...props }: React.ComponentProps<typeof SheetPrimitive.Description>) {
    return <SheetPrimitive.Description className={cn('mt-1 text-sm text-muted-foreground', className)} {...props} />
}

export { Sheet, SheetTrigger, SheetClose, SheetContent, SheetHeader, SheetTitle, SheetDescription }
