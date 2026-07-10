import { StartBootstrapForm } from '../components/start-bootstrap-form'

export function BootstrapStartPage() {
    return (
        <section className="bg-card rounded-lg border p-6 shadow-sm sm:p-8">
            <p className="text-sm font-medium text-[#328f97]">Step 1 of 3</p>
            <h1 className="mt-2 text-2xl font-semibold">Claim this installation</h1>
            <p className="text-muted-foreground mt-2 mb-7 text-sm">Validate the deployment secret and choose the first administrator email.</p>
            <StartBootstrapForm />
        </section>
    )
}
