import { useRouter } from '@tanstack/react-router'

import { Button } from '@/components/ui/button'

export function RoutePending() {
    return <main className="text-muted-foreground flex min-h-screen items-center justify-center p-6 text-sm">Loading Veritas...</main>
}

export function RouteError() {
    const router = useRouter()

    return (
        <main className="flex min-h-screen items-center justify-center p-6">
            <section className="bg-card w-full max-w-md rounded-lg border p-6 text-center shadow-sm">
                <h1 className="text-xl font-semibold">Veritas could not load this page</h1>
                <p className="text-muted-foreground mt-2 mb-5 text-sm">The service may be temporarily unavailable. No setup state was assumed.</p>
                <Button
                    onClick={() => {
                        void router.invalidate()
                    }}
                >
                    Try again
                </Button>
            </section>
        </main>
    )
}
