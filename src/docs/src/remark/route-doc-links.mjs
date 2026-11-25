import path from 'node:path'

/**
 * Rewrites repository-friendly Markdown links to their deployed documentation routes.
 *
 * Source documentation can continue linking to sibling `.md` and `.mdx` files so
 * it renders correctly on GitHub. The built site instead receives clean,
 * base-prefixed URLs suitable for GitHub Pages.
 */
export function routeDocumentationLinks({ base, docsRoot }) {
    const absoluteDocsRoot = path.resolve(docsRoot)
    const normalizedBase = `/${base.replace(/^\/+|\/+$/g, '')}`

    return (tree, file) => {
        visitLinks(tree, (node) => {
            if (typeof node.url !== 'string' || node.url.startsWith('#') || /^[a-z][a-z\d+.-]*:/i.test(node.url)) {
                return
            }

            const match = node.url.match(/^([^?#]+?\.(?:md|mdx))([?#].*)?$/i)
            if (!match) {
                return
            }

            const sourcePath = file.path ? path.dirname(file.path) : absoluteDocsRoot
            const targetPath = path.resolve(sourcePath, match[1])
            const relativePath = path.relative(absoluteDocsRoot, targetPath)

            if (relativePath.startsWith('..') || path.isAbsolute(relativePath)) {
                return
            }

            const pathWithoutExtension = relativePath.replace(/\.(?:md|mdx)$/i, '')
            const routeSegments = pathWithoutExtension
                .split(path.sep)
                .filter(Boolean)
                .map((segment) => segment.toLowerCase())

            if (routeSegments.at(-1) === 'readme') {
                routeSegments.pop()
            }

            const route = routeSegments.length > 0 ? `${routeSegments.join('/')}/` : 'docs/'
            node.url = `${normalizedBase}/${route}${match[2] ?? ''}`
        })
    }
}

function visitLinks(node, visitor) {
    if (!node || typeof node !== 'object') {
        return
    }

    if (node.type === 'link') {
        visitor(node)
    }

    if (Array.isArray(node.children)) {
        for (const child of node.children) {
            visitLinks(child, visitor)
        }
    }
}
