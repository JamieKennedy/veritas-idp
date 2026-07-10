import { Link, Outlet, useNavigate } from '@tanstack/react-router'
import { useQueryClient } from '@tanstack/react-query'
import { LayoutDashboard, LogOut, Mail, ShieldCheck } from 'lucide-react'
import { useState } from 'react'

import { logoutAdmin } from '@/features/auth/api/auth.api'
import type { AdminIdentity } from '@/features/auth/model/auth.schemas'
import { Button } from '@/components/ui/button'

export function AdminShell({ admin }: { admin: AdminIdentity }) {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [isLoggingOut, setIsLoggingOut] = useState(false)
    const [logoutError, setLogoutError] = useState(false)

    async function handleLogout() {
        setIsLoggingOut(true)
        setLogoutError(false)
        try {
            await logoutAdmin()
            queryClient.clear()
            await navigate({ to: '/login', search: { redirect: '/dashboard' } })
        } catch {
            setLogoutError(true)
        } finally {
            setIsLoggingOut(false)
        }
    }

    return (
        <div className="bg-background grid min-h-screen md:grid-cols-[15rem_1fr]">
            <aside className="border-b bg-[#173a40] px-4 py-5 text-white md:border-r md:border-b-0">
                <div className="flex items-center gap-2 px-2 text-lg font-semibold">
                    <ShieldCheck className="size-6 text-[#60d7cf]" /> Veritas
                </div>
                <nav className="mt-8 flex gap-2 md:flex-col">
                    <Button asChild variant="ghost" className="justify-start text-white hover:bg-white/10 hover:text-white">
                        <Link to="/dashboard">
                            <LayoutDashboard /> Dashboard
                        </Link>
                    </Button>
                    <Button asChild variant="ghost" className="justify-start text-white hover:bg-white/10 hover:text-white">
                        <Link to="/bootstrap/smtp">
                            <Mail /> Email setup
                        </Link>
                    </Button>
                </nav>
            </aside>
            <div className="min-w-0">
                <header className="bg-card flex min-h-16 items-center justify-between border-b px-5">
                    <div>
                        <p className="text-sm font-medium">{admin.name ?? admin.email}</p>
                        <p className="text-muted-foreground text-xs">{admin.email}</p>
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        disabled={isLoggingOut}
                        onClick={() => {
                            void handleLogout()
                        }}
                    >
                        <LogOut /> {isLoggingOut ? 'Signing out...' : 'Sign out'}
                    </Button>
                </header>
                {logoutError && (
                    <p role="alert" className="bg-destructive/5 text-destructive border-b px-5 py-3 text-sm">
                        Sign out could not be completed. Please try again.
                    </p>
                )}
                <main className="p-5 md:p-8">
                    <Outlet />
                </main>
            </div>
        </div>
    )
}
