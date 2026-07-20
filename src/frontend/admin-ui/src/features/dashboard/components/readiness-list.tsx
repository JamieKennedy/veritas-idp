import { CheckCircle2, CircleAlert } from 'lucide-react'

export function ReadinessList({ isSmtpConfigured }: { isSmtpConfigured: boolean }) {
    const items = [
        { label: 'Installation claimed', ready: true },
        { label: 'Administrator configured', ready: true },
        { label: 'Multi-factor authentication active', ready: true },
        { label: 'Email delivery configured', ready: isSmtpConfigured },
    ]

    return (
        <section className="bg-card rounded-lg border">
            <h2 className="border-b px-5 py-4 font-medium">Instance readiness</h2>
            <ul className="divide-y">
                {items.map((item) => (
                    <li key={item.label} className="flex items-center justify-between px-5 py-4 text-sm">
                        {item.label}
                        <span className={item.ready ? 'flex items-center gap-2 text-emerald-700' : 'flex items-center gap-2 text-amber-700'}>
                            {item.ready ? <CheckCircle2 className="size-4" /> : <CircleAlert className="size-4" />}
                            {item.ready ? 'Ready' : 'Attention'}
                        </span>
                    </li>
                ))}
            </ul>
        </section>
    )
}
