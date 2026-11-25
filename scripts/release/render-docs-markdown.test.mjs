import assert from 'node:assert/strict'
import test from 'node:test'

import { renderDocsMarkdown } from './render-docs-markdown.mjs'

test('renders site frontmatter as an ordinary Markdown heading', () => {
    const source = `---
title: Docker Compose deployment
description: Deploy Veritas.
section: Deploy and operate
sectionOrder: 1
order: 0
---

Deployment content.
`

    assert.equal(renderDocsMarkdown(source), '# Docker Compose deployment\n\nDeployment content.\n')
})

test('rewrites site-only links for the release bundle', () => {
    const source = `---
title: Deployment
---

Read [Secret management](./secrets.md).
`

    assert.equal(
        renderDocsMarkdown(source, {
            replacements: {
                './secrets.md': './secrets/README.md',
            },
        }),
        '# Deployment\n\nRead [Secret management](./secrets/README.md).\n',
    )
})

test('preserves ordinary Markdown while normalizing its final newline', () => {
    assert.equal(renderDocsMarkdown('# Existing guide\r\n\r\nContent.'), '# Existing guide\n\nContent.\n')
})

test('removes site-only shell tab markers from release guides', () => {
    const source = `---
title: Install
---

<div data-shell-tabs="install"></div>

\`\`\`bash
docker compose up -d
\`\`\`
`

    assert.equal(renderDocsMarkdown(source), '# Install\n\n```bash\ndocker compose up -d\n```\n')
})
