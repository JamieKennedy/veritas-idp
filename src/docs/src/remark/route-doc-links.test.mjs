import path from 'node:path'
import { describe, expect, it } from 'vitest'
import { routeDocumentationLinks } from './route-doc-links.mjs'

describe('routeDocumentationLinks', () => {
    const docsRoot = path.resolve('test-fixtures/docs')
    const transform = routeDocumentationLinks({
        base: '/veritas-idp/',
        docsRoot,
    })

    it('converts relative Markdown links to base-prefixed routes', () => {
        const tree = {
            type: 'root',
            children: [
                {
                    type: 'link',
                    url: '../operations/environment.mdx#precedence',
                    children: [],
                },
            ],
        }

        transform(tree, {
            path: path.join(docsRoot, 'getting-started', 'requirements.md'),
        })

        expect(tree.children[0].url).toBe('/veritas-idp/operations/environment/#precedence')
    })

    it('maps a README to its directory route', () => {
        const tree = {
            type: 'root',
            children: [{ type: 'link', url: './README.md', children: [] }],
        }

        transform(tree, { path: path.join(docsRoot, 'guide.md') })

        expect(tree.children[0].url).toBe('/veritas-idp/docs/')
    })

    it('leaves external and non-Markdown links unchanged', () => {
        const tree = {
            type: 'root',
            children: [
                {
                    type: 'link',
                    url: 'https://example.com/guide.md',
                    children: [],
                },
                { type: 'link', url: './diagram.svg', children: [] },
            ],
        }

        transform(tree, { path: path.join(docsRoot, 'guide.md') })

        expect(tree.children.map(({ url }) => url)).toEqual(['https://example.com/guide.md', './diagram.svg'])
    })
})
