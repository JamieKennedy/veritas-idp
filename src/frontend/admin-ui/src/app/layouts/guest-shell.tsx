import { Outlet } from '@tanstack/react-router'
import { ShieldCheck } from 'lucide-react'

export function GuestShell() {
    return (
        <main className="bg-background flex min-h-screen items-center justify-center px-5 py-10">
            <div className="w-full max-w-md">
                <div className="mb-8 flex items-center justify-center gap-3 text-xl font-semibold">
                    <ShieldCheck className="size-7 text-[#328f97]" />
                    Veritas
                </div>
                <Outlet />
            </div>
        </main>
    )
}
