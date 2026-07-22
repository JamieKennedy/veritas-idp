import { execFileSync } from 'node:child_process'
import fs from 'node:fs'
import path from 'node:path'

const repositoryRoot = path.resolve(import.meta.dirname, '../..')
const markdownFiles = execFileSync('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', '*.md'], {
    cwd: repositoryRoot,
    encoding: 'utf8',
})
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)

const linkPattern = /!?\[[^\]]*\]\((?<target><[^>]+>|[^\s)]+)(?:\s+["'][^"']*["'])?\)/g
const failures = []

for (const relativeFile of markdownFiles) {
    const absoluteFile = path.resolve(repositoryRoot, relativeFile)
    const contents = fs.readFileSync(absoluteFile, 'utf8')

    for (const match of contents.matchAll(linkPattern)) {
        const rawTarget = match.groups?.target?.replace(/^<|>$/g, '')
        if (rawTarget === undefined || rawTarget.startsWith('#') || /^[a-z][a-z\d+.-]*:/i.test(rawTarget)) {
            continue
        }

        const pathComponent = rawTarget.split(/[?#]/, 1)[0]
        let decodedTarget
        try {
            decodedTarget = decodeURIComponent(pathComponent)
        } catch {
            failures.push(`${relativeFile}: malformed link target ${rawTarget}`)
            continue
        }

        const absoluteTarget = decodedTarget.startsWith('/')
            ? path.resolve(repositoryRoot, decodedTarget.slice(1))
            : path.resolve(path.dirname(absoluteFile), decodedTarget)
        const targetRelativeToRoot = path.relative(repositoryRoot, absoluteTarget)

        if (targetRelativeToRoot.startsWith('..') || path.isAbsolute(targetRelativeToRoot)) {
            failures.push(`${relativeFile}: local link leaves the repository: ${rawTarget}`)
            continue
        }

        if (!fs.existsSync(absoluteTarget)) {
            failures.push(`${relativeFile}: missing local link target ${rawTarget}`)
        }
    }
}

if (failures.length > 0) {
    console.error(['Markdown link validation failed:', ...failures.map((failure) => `- ${failure}`)].join('\n'))
    process.exit(1)
}

console.log(`Validated local links in ${markdownFiles.length} Markdown files.`)
