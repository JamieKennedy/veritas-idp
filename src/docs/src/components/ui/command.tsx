import type * as React from 'react'
import { Command as CommandPrimitive } from 'cmdk'
import { Search } from 'lucide-react'

import { cn } from '@/lib/utils'

function Command({ className, ...props }: React.ComponentProps<typeof CommandPrimitive>) {
    return <CommandPrimitive className={cn('flex size-full flex-col overflow-hidden rounded-xl bg-popover text-popover-foreground', className)} {...props} />
}

function CommandInput({ className, ...props }: React.ComponentProps<typeof CommandPrimitive.Input>) {
    return (
        <div className="flex items-center gap-2 border-b px-3">
            <Search className="size-4 shrink-0 text-muted-foreground" />
            <CommandPrimitive.Input
                className={cn('h-12 w-full bg-transparent text-sm outline-none placeholder:text-muted-foreground disabled:opacity-50', className)}
                {...props}
            />
        </div>
    )
}

function CommandList({ className, ...props }: React.ComponentProps<typeof CommandPrimitive.List>) {
    return <CommandPrimitive.List className={cn('max-h-[24rem] overflow-x-hidden overflow-y-auto p-2', className)} {...props} />
}

function CommandEmpty({ className, ...props }: React.ComponentProps<typeof CommandPrimitive.Empty>) {
    return <CommandPrimitive.Empty className={cn('py-10 text-center text-sm text-muted-foreground', className)} {...props} />
}

function CommandGroup({ className, ...props }: React.ComponentProps<typeof CommandPrimitive.Group>) {
    return <CommandPrimitive.Group className={cn('overflow-hidden p-1 text-foreground', className)} {...props} />
}

function CommandItem({ className, ...props }: React.ComponentProps<typeof CommandPrimitive.Item>) {
    return (
        <CommandPrimitive.Item
            className={cn(
                'relative flex cursor-default items-start gap-3 rounded-lg px-3 py-2.5 text-sm outline-none data-[selected=true]:bg-accent data-[selected=true]:text-accent-foreground',
                className,
            )}
            {...props}
        />
    )
}

export { Command, CommandInput, CommandList, CommandEmpty, CommandGroup, CommandItem }
