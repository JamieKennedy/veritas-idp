export interface LandingLink {
    href: string
    label: string
}

export interface LandingCapability {
    description: string
    title: string
}

export interface LandingContent {
    available: Array<LandingCapability>
    description: string
    differentiators: Array<LandingCapability>
    eyebrow: string
    install: {
        commands: {
            bash: Array<string>
            powershell: Array<string>
        }
        description: string
    }
    links: {
        architecture: LandingLink
        contribute: LandingLink
        documentation: LandingLink
        install: LandingLink
        repository: LandingLink
    }
    roadmap: Array<LandingCapability>
    technologies: Array<string>
    title: string
    warning: string
}

export const landingContent: LandingContent = {
    eyebrow: 'Open-source identity infrastructure',
    title: 'Own the identity layer your applications depend on.',
    description:
        'Veritas is an experimental, self-hostable authentication service and identity provider for teams that want inspectable code, deployment control, and a clear path away from identity-platform lock-in.',
    warning: 'Veritas is pre-alpha software. It must not be used to protect production systems or real user identities.',
    links: {
        documentation: { href: '/docs/', label: 'Read the documentation' },
        install: {
            href: '/operations/docker-compose/',
            label: 'Install with Docker Compose',
        },
        contribute: {
            href: 'https://github.com/JamieKennedy/veritas-idp/blob/staging/CONTRIBUTING.md',
            label: 'Contribute',
        },
        architecture: {
            href: '/engineering/architecture/',
            label: 'Explore the architecture',
        },
        repository: {
            href: 'https://github.com/JamieKennedy/veritas-idp',
            label: 'View source on GitHub',
        },
    },
    differentiators: [
        {
            title: 'Operate it yourself',
            description: 'Run the complete stack on infrastructure you control, with Docker Compose packaging and explicit operational boundaries.',
        },
        {
            title: 'Inspect every trust boundary',
            description:
                'Review the authentication flow, persistence model, secret handling, and deployment topology instead of treating identity as a black box.',
        },
        {
            title: 'Build on open foundations',
            description: 'Use an AGPL-licensed codebase with modular .NET services, React administration surfaces, and versioned integration contracts.',
        },
    ],
    available: [
        {
            title: 'Guarded bootstrap',
            description: 'A time-limited, secret-protected path creates the first administrator and closes after setup.',
        },
        {
            title: 'Administrator MFA',
            description: 'Password verification, mandatory TOTP enrollment, recovery codes, server-side sessions, and CSRF protection.',
        },
        {
            title: 'Protected SMTP configuration',
            description: 'Configure and test provider-scoped SMTP credentials through the administration flow without read-back of the secret.',
        },
        {
            title: 'Production-shaped packaging',
            description:
                'Immutable application images, one-shot database migrations, health checks, persistent keys, and a self-contained Compose release bundle.',
        },
    ],
    roadmap: [
        {
            title: 'OAuth 2.0 and OpenID Connect',
            description: 'Authorization, token, discovery, signing-key, and client-management capabilities are not implemented yet.',
        },
        {
            title: 'End-user identity flows',
            description: 'Registration, sign-in, account recovery, consent, sessions, federation, and the Public UI remain future work.',
        },
        {
            title: 'Stable production guarantees',
            description: 'Upgrade compatibility, long-term support, and production security assurances will arrive only after the protocol surface matures.',
        },
    ],
    install: {
        description: 'Start from an immutable release bundle, generate independent secrets, validate the resolved topology, and then launch the stack.',
        commands: {
            bash: ['cp .env.example .env', 'mkdir -p secrets', 'openssl rand -hex 32', 'docker compose config --quiet', 'docker compose up -d'],
            powershell: [
                'Copy-Item .env.example .env',
                'New-Item -ItemType Directory -Force secrets',
                'openssl rand -hex 32',
                'docker compose config --quiet',
                'docker compose up -d',
            ],
        },
    },
    technologies: ['.NET 11', 'ASP.NET Core', 'React', 'Astro', 'PostgreSQL', 'RabbitMQ', 'Redis', 'Docker Compose', '.NET Aspire'],
}
