import { execFileSync } from 'node:child_process'
import path from 'node:path'

import type { CollectionEntry } from 'astro:content'

export type DocumentationEntry = CollectionEntry<'docs'>

export interface NavigationItem {
    description: string
    href: string
    id: string
    label: string
}

export interface NavigationSection {
    items: Array<NavigationItem>
    label: string
    order: number
}

export function documentationPath(entry: DocumentationEntry) {
    return entry.id === 'index' ? '/docs/' : `/${entry.id}/`
}

export function visibleDocumentation(entries: Array<DocumentationEntry>) {
    return entries
        .filter((entry) => !entry.data.draft)
        .sort(
            (left, right) =>
                left.data.sectionOrder - right.data.sectionOrder || left.data.order - right.data.order || left.data.title.localeCompare(right.data.title),
        )
}

export function createDocumentationNavigation(entries: Array<DocumentationEntry>): Array<NavigationSection> {
    const sections = new Map<string, NavigationSection>()

    for (const entry of visibleDocumentation(entries)) {
        const existing = sections.get(entry.data.section) ?? {
            items: [],
            label: entry.data.section,
            order: entry.data.sectionOrder,
        }

        if (existing.order !== entry.data.sectionOrder) {
            throw new Error(`Documentation section "${entry.data.section}" uses conflicting sectionOrder values.`)
        }

        existing.items.push({
            description: entry.data.description,
            href: documentationPath(entry),
            id: entry.id,
            label: entry.data.label ?? entry.data.title,
        })
        sections.set(entry.data.section, existing)
    }

    return [...sections.values()].sort((left, right) => left.order - right.order || left.label.localeCompare(right.label))
}

export function adjacentDocumentation(entries: Array<DocumentationEntry>, currentId: string) {
    const ordered = visibleDocumentation(entries)
    const index = ordered.findIndex((entry) => entry.id === currentId)

    return {
        next: index >= 0 ? ordered[index + 1] : undefined,
        previous: index > 0 ? ordered[index - 1] : undefined,
    }
}

export function documentationSourcePath(entry: DocumentationEntry) {
    const filePath = 'filePath' in entry && typeof entry.filePath === 'string' ? entry.filePath : undefined
    if (filePath) {
        return path.relative(path.resolve(process.cwd(), '../..'), filePath).replaceAll('\\', '/')
    }

    return entry.id === 'index' ? 'docs/README.md' : `docs/${entry.id}.md`
}

export function documentationEditUrl(entry: DocumentationEntry) {
    return `https://github.com/JamieKennedy/veritas-idp/edit/staging/${documentationSourcePath(entry)}`
}

export function documentationLastUpdated(entry: DocumentationEntry) {
    try {
        const output = execFileSync('git', ['log', '-1', '--format=%cI', '--', documentationSourcePath(entry)], {
            cwd: path.resolve(process.cwd(), '../..'),
            encoding: 'utf8',
            stdio: ['ignore', 'pipe', 'ignore'],
        }).trim()

        return output.length > 0 ? new Date(output) : undefined
    } catch {
        return undefined
    }
}
