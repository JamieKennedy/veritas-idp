import { defineCollection } from 'astro:content'
import { glob } from 'astro/loaders'
import { z } from 'astro/zod'

function generateDocumentationId(entry: string) {
    if (entry.toLowerCase() === 'readme.md') {
        return 'index'
    }

    return entry
        .replace(/\.mdx?$/i, '')
        .split('/')
        .map((segment) =>
            segment
                .toLowerCase()
                .replaceAll(/[^a-z0-9]+/g, '-')
                .replaceAll(/^-|-$/g, ''),
        )
        .join('/')
}

export const collections = {
    docs: defineCollection({
        loader: glob({
            base: new URL('../../../docs/', import.meta.url),
            pattern: '**/*.{md,mdx}',
            generateId: ({ entry }) => generateDocumentationId(entry),
        }),
        schema: z.object({
            title: z.string().min(1),
            description: z.string().min(1),
            section: z.string().min(1),
            sectionOrder: z.number().int().nonnegative(),
            order: z.number().int().nonnegative(),
            label: z.string().min(1).optional(),
            draft: z.boolean().default(false),
        }),
    }),
}
