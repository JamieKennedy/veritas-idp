import { unified } from '@astrojs/markdown-remark'
import mdx from '@astrojs/mdx'
import react from '@astrojs/react'
import tailwindcss from '@tailwindcss/vite'
import { defineConfig } from 'astro/config'
import { fileURLToPath } from 'node:url'
import { routeDocumentationLinks } from './src/remark/route-doc-links.mjs'

const docsRoot = fileURLToPath(new URL('../../docs/', import.meta.url))

export default defineConfig({
    site: 'https://jamiekennedy.github.io',
    base: '/veritas-idp',
    publicDir: '../../assets/brand',
    trailingSlash: 'always',
    markdown: {
        processor: unified({
            remarkPlugins: [[routeDocumentationLinks, { base: '/veritas-idp', docsRoot }]],
        }),
        shikiConfig: {
            themes: {
                light: 'github-light',
                dark: 'github-dark',
            },
            wrap: true,
        },
    },
    vite: {
        plugins: [tailwindcss()],
    },
    integrations: [react(), mdx()],
})
