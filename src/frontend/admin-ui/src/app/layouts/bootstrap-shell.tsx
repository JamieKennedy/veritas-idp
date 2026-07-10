import { Outlet } from '@tanstack/react-router'
import { CheckCircle2, Circle, ShieldCheck } from 'lucide-react'

export function BootstrapShell() {
    return (
        <div className="bg-background grid min-h-screen lg:grid-cols-[18rem_1fr]">
            <aside className="border-b bg-[#173a40] px-6 py-7 text-white lg:border-r lg:border-b-0">
                <div className="flex items-center gap-3 text-lg font-semibold">
                    <ShieldCheck className="size-6 text-[#60d7cf]" />
                    Veritas
                </div>
                <p className="mt-2 text-sm text-white/65">Installation setup</p>
                <ol className="mt-10 space-y-6 text-sm">
                    <li className="flex items-center gap-3">
                        <CheckCircle2 className="size-5 text-[#60d7cf]" /> Claim installation
                    </li>
                    <li className="flex items-center gap-3">
                        <Circle className="size-5 text-white/45" /> Secure administrator
                    </li>
                    <li className="flex items-center gap-3">
                        <Circle className="size-5 text-white/45" /> Configure email
                    </li>
                </ol>
            </aside>
            <main className="flex items-center justify-center px-5 py-10">
                <div className="w-full max-w-2xl">
                    <Outlet />
                </div>
            </main>
        </div>
    )
}
