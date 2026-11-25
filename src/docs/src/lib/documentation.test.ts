import { describe, expect, it } from 'vitest'

import { adjacentDocumentation, createDocumentationNavigation, documentationPath } from './documentation'

function entry(id: string, section: string, sectionOrder: number, order: number, draft = false) {
    return {
        id,
        collection: 'docs',
        data: {
            title: id,
            description: `${id} description`,
            section,
            sectionOrder,
            order,
            draft,
        },
    }
}

describe('documentation navigation', () => {
    const entries = [
        entry('operations/environment', 'Operate', 1, 1),
        entry('index', 'Start here', 0, 0),
        entry('operations/docker-compose', 'Operate', 1, 0),
        entry('draft', 'Start here', 0, 2, true),
    ]

    it('preserves stable guide paths and maps the overview to /docs/', () => {
        expect(documentationPath(entries[0] as never)).toBe('/operations/environment/')
        expect(documentationPath(entries[1] as never)).toBe('/docs/')
    })

    it('generates deterministic sections and excludes drafts', () => {
        const navigation = createDocumentationNavigation(entries as never)

        expect(navigation.map((section) => section.label)).toEqual(['Start here', 'Operate'])
        expect(navigation[1].items.map((item) => item.id)).toEqual(['operations/docker-compose', 'operations/environment'])
        expect(navigation.flatMap((section) => section.items).some((item) => item.id === 'draft')).toBe(false)
    })

    it('returns previous and next pages from the same canonical ordering', () => {
        const adjacent = adjacentDocumentation(entries as never, 'operations/docker-compose')

        expect(adjacent.previous?.id).toBe('index')
        expect(adjacent.next?.id).toBe('operations/environment')
    })
})
