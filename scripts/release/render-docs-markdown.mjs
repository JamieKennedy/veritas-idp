export function renderDocsMarkdown(source, { replacements = {} } = {}) {
    const normalized = source.replaceAll('\r\n', '\n')
    const frontmatterMatch = normalized.match(/^---\n(?<frontmatter>[\s\S]*?)\n---\n(?<body>[\s\S]*)$/)

    let rendered = normalized
    if (frontmatterMatch?.groups) {
        const titleMatch = frontmatterMatch.groups.frontmatter.match(/^title:\s*(?<title>.+)$/m)
        if (!titleMatch?.groups?.title) {
            throw new Error('Documentation frontmatter must include a title before it can be packaged.')
        }

        const title = titleMatch.groups.title.trim().replace(/^(['"])(.*)\1$/, '$2')
        rendered = `# ${title}\n\n${frontmatterMatch.groups.body.trimStart()}`
    }

    for (const [search, replacement] of Object.entries(replacements)) {
        rendered = rendered.replaceAll(search, replacement)
    }

    rendered = rendered.replace(/<div data-shell-tabs="[^"]+"><\/div>\n\n/g, '')

    return `${rendered.trimEnd()}\n`
}
