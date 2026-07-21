import { StartBootstrapForm } from '../components/start-bootstrap-form'

export function BootstrapStartPage() {
    return (
        <section className="setup-card p-6 sm:p-8">
            <p className="setup-card__eyebrow">Installation setup</p>
            <h1 className="mt-2 text-2xl font-semibold">Claim this installation</h1>
            <p className="text-muted-foreground mt-2 mb-7 text-sm">Validate the deployment secret and choose the first administrator email.</p>
            <StartBootstrapForm />
        </section>
    )
}
