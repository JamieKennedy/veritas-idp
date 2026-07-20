const defaultAlgorithm = 'SHA1'
const defaultDigits = 6
const defaultPeriodSeconds = 30

export interface TotpProvisioningDetails {
    type: 'Time-based (TOTP)'
    secretFormat: 'Base32'
    algorithm: string
    digits: number
    periodSeconds: number
    issuer?: string
    account?: string
}

export function parseTotpProvisioningUri(provisioningUri: string | null): TotpProvisioningDetails | null {
    if (!provisioningUri) {
        return null
    }

    try {
        const uri = new URL(provisioningUri)
        if (uri.protocol !== 'otpauth:' || uri.hostname.toLowerCase() !== 'totp') {
            return null
        }

        const label = decodeURIComponent(uri.pathname.replace(/^\/+/, ''))
        const separatorIndex = label.indexOf(':')
        const labelIssuer = separatorIndex > 0 ? label.slice(0, separatorIndex) : undefined
        const accountLabel = separatorIndex >= 0 ? label.slice(separatorIndex + 1).trim() : label.trim()
        const account = accountLabel.length > 0 ? accountLabel : undefined
        const issuerParameter = uri.searchParams.get('issuer')?.trim()
        const issuer = issuerParameter && issuerParameter.length > 0 ? issuerParameter : labelIssuer
        const algorithmParameter = uri.searchParams.get('algorithm')?.trim()

        return {
            type: 'Time-based (TOTP)',
            secretFormat: 'Base32',
            algorithm: formatAlgorithm(algorithmParameter && algorithmParameter.length > 0 ? algorithmParameter : defaultAlgorithm),
            digits: parsePositiveInteger(uri.searchParams.get('digits'), defaultDigits),
            periodSeconds: parsePositiveInteger(uri.searchParams.get('period'), defaultPeriodSeconds),
            issuer,
            account,
        }
    } catch {
        return null
    }
}

function formatAlgorithm(algorithm: string) {
    return algorithm.toUpperCase().replace(/^SHA-?(\d+)$/, 'SHA-$1')
}

function parsePositiveInteger(value: string | null, fallback: number) {
    if (value === null || !/^\d+$/.test(value)) {
        return fallback
    }

    const parsed = Number.parseInt(value, 10)
    return parsed > 0 ? parsed : fallback
}
