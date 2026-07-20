import { CompleteBootstrapForm } from '../components/complete-bootstrap-form'

export function BootstrapCompletePage() {
    return (
        <section className="setup-card p-6 sm:p-8">
            <p className="setup-card__eyebrow">Installation setup</p>
            <h1 className="mt-2 text-2xl font-semibold">Secure the first administrator</h1>
            <p className="text-muted-foreground mt-2 mb-7 text-sm">Create the administrator credentials. Normal login and MFA enrollment follow next.</p>
            <CompleteBootstrapForm />
        </section>
    )
}
