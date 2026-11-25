export const repositoryUrl = 'https://github.com/JamieKennedy/veritas-idp'
export const documentationUrl = 'https://jamiekennedy.github.io/veritas-idp/'

export function withBase(pathname = '/') {
    const base = import.meta.env.BASE_URL.replace(/\/$/, '')
    const normalizedPath = pathname.startsWith('/') ? pathname : `/${pathname}`
    return `${base}${normalizedPath}`
}

export function isExternalUrl(value: string) {
    return /^https?:\/\//i.test(value)
}
