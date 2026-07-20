// @ts-check

const apiPathPrefix = '/api/'
const healthPath = '/healthz'
const hopByHopHeaders = ['connection', 'keep-alive', 'proxy-authenticate', 'proxy-authorization', 'te', 'trailer', 'transfer-encoding', 'upgrade']

/**
 * @typedef {{ fetch(request: Request): Response | Promise<Response> }} FetchHandler
 */

/**
 * @param {string | undefined} configuredOrigin
 * @returns {URL}
 */
export function parseAdminApiOrigin(configuredOrigin) {
    if (configuredOrigin === undefined || configuredOrigin.trim().length === 0) {
        throw new Error('ADMIN_API_ORIGIN is required.')
    }

    const origin = new URL(configuredOrigin)
    const hasRootPath = origin.pathname === '/' && origin.search.length === 0 && origin.hash.length === 0
    const usesHttp = origin.protocol === 'http:' || origin.protocol === 'https:'

    if (!usesHttp || !hasRootPath || origin.username.length > 0 || origin.password.length > 0) {
        throw new Error('ADMIN_API_ORIGIN must be an HTTP(S) origin without credentials, a path, a query, or a fragment.')
    }

    return origin
}

/**
 * @param {{
 *   adminApiOrigin: URL
 *   application: FetchHandler
 *   fetchImplementation?: typeof fetch
 * }} options
 * @returns {FetchHandler}
 */
export function createServerHandler({ adminApiOrigin, application, fetchImplementation = fetch }) {
    return {
        async fetch(request) {
            const requestUrl = new URL(request.url)

            if (requestUrl.pathname === healthPath) {
                return new Response(null, {
                    status: 204,
                    headers: {
                        'Cache-Control': 'no-store',
                    },
                })
            }

            if (requestUrl.pathname !== '/api' && !requestUrl.pathname.startsWith(apiPathPrefix)) {
                return application.fetch(request)
            }

            const targetUrl = new URL(`${requestUrl.pathname}${requestUrl.search}`, adminApiOrigin)
            const headers = createForwardHeaders(request.headers, requestUrl)
            /** @type {RequestInit & { duplex?: 'half' }} */
            const requestInit = {
                method: request.method,
                headers,
                redirect: 'manual',
                signal: request.signal,
            }

            if (request.method !== 'GET' && request.method !== 'HEAD') {
                requestInit.body = request.body
                requestInit.duplex = 'half'
            }

            return fetchImplementation(targetUrl, requestInit)
        },
    }
}

/**
 * @param {Headers} requestHeaders
 * @param {URL} requestUrl
 * @returns {Headers}
 */
function createForwardHeaders(requestHeaders, requestUrl) {
    const headers = new Headers(requestHeaders)

    for (const header of hopByHopHeaders) {
        headers.delete(header)
    }

    headers.delete('host')
    headers.set('x-forwarded-host', requestHeaders.get('x-forwarded-host') ?? requestUrl.host)
    headers.set('x-forwarded-proto', requestHeaders.get('x-forwarded-proto') ?? requestUrl.protocol.slice(0, -1))

    return headers
}
