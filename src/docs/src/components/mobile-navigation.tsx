import { Menu } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { ScrollArea } from '@/components/ui/scroll-area'
import { Separator } from '@/components/ui/separator'
import { Sheet, SheetClose, SheetContent, SheetDescription, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet'
import type { NavigationSection } from '@/lib/documentation'

interface MobileNavigationProps {
    baseUrl: string
    currentPath: string
    navigation: Array<NavigationSection>
}

export function MobileNavigation({ baseUrl, currentPath, navigation }: MobileNavigationProps) {
    return (
        <Sheet>
            <SheetTrigger asChild>
                <Button type="button" variant="ghost" size="icon" className="lg:hidden" aria-label="Open documentation navigation">
                    <Menu />
                </Button>
            </SheetTrigger>
            <SheetContent side="left">
                <SheetHeader>
                    <SheetTitle>Veritas documentation</SheetTitle>
                    <SheetDescription>Deployment, operations, security, and engineering guides.</SheetDescription>
                </SheetHeader>
                <ScrollArea className="min-h-0 flex-1">
                    <nav aria-label="Mobile documentation" className="space-y-6 p-5">
                        {navigation.map((section) => (
                            <section key={section.label}>
                                <h2 className="mb-2 text-xs font-semibold tracking-[0.12em] text-muted-foreground uppercase">{section.label}</h2>
                                <ul className="space-y-1">
                                    {section.items.map((item) => {
                                        const href = `${baseUrl}${item.href}`
                                        const active = currentPath === href
                                        return (
                                            <li key={item.id}>
                                                <SheetClose asChild>
                                                    <a
                                                        href={href}
                                                        aria-current={active ? 'page' : undefined}
                                                        className="block rounded-lg px-3 py-2 text-sm font-medium hover:bg-accent aria-[current=page]:bg-primary/10 aria-[current=page]:text-primary"
                                                    >
                                                        {item.label}
                                                    </a>
                                                </SheetClose>
                                            </li>
                                        )
                                    })}
                                </ul>
                            </section>
                        ))}
                    </nav>
                </ScrollArea>
                <Separator />
                <div className="p-5 text-sm">
                    <a className="font-semibold text-primary hover:underline" href="https://github.com/JamieKennedy/veritas-idp">
                        View the source on GitHub
                    </a>
                </div>
            </SheetContent>
        </Sheet>
    )
}
