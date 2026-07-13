import { Link, Outlet, useNavigate } from '@tanstack/react-router'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { LayoutDashboard, LogOut, Mail } from 'lucide-react'
import { useState } from 'react'

import veritasLockupUrl from '@brand/veritas-lockup.svg?url'

import { logoutAdminMutationOptions } from '@/features/auth/api/auth.api'
import { getInlineRequestError } from '@/lib/api/request-errors'
import type { AdminIdentity } from '@/features/auth/model/auth.schemas'
import { ThemeControl } from '@/app/theme/theme-control'
import { Button } from '@/components/ui/button'

export function AdminShell({ admin }: { admin: AdminIdentity }) {
    const navigate = useNavigate()
    const queryClient = useQueryClient()
    const [logoutError, setLogoutError] = useState(false)
    const logoutMutation = useMutation(logoutAdminMutationOptions())

    async function handleLogout() {
        setLogoutError(false)
        try {
            await logoutMutation.mutateAsync()
            queryClient.clear()
            await navigate({ to: '/login', search: { redirect: '/dashboard' } })
        } catch (error) {
            setLogoutError(getInlineRequestError(error) !== undefined)
        }
    }

    return (
        <div className="bg-background grid min-h-screen md:grid-cols-[15rem_1fr]">
            <aside className="admin-sidebar border-b px-4 py-5 text-white md:border-r md:border-b-0">
                <div className="admin-sidebar__brand">
                    <img src={veritasLockupUrl} alt="Veritas" className="admin-sidebar__lockup" />
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
                    <div className="flex items-center gap-3">
                        <ThemeControl />
                        <Button
                            variant="outline"
                            size="sm"
                            disabled={logoutMutation.isPending}
                            onClick={() => {
                                void handleLogout()
                            }}
                        >
                            <LogOut /> {logoutMutation.isPending ? 'Signing out...' : 'Sign out'}
                        </Button>
                    </div>
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
