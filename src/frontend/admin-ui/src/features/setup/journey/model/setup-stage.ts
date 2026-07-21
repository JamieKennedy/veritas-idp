export const setupStages = [
    { id: 'claim', label: 'Claim installation' },
    { id: 'secure', label: 'Secure administrator' },
    { id: 'email', label: 'Configure email' },
] as const

export type SetupStage = (typeof setupStages)[number]['id']
