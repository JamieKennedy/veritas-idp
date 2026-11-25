import { readFileSync } from 'node:fs'
import path from 'node:path'

import { describe, expect, it } from 'vitest'

const composeFile = readFileSync(path.resolve(process.cwd(), '../../compose.yaml'), 'utf8')
const environmentExample = readFileSync(path.resolve(process.cwd(), '../../.env.example'), 'utf8')
const environmentGuide = readFileSync(path.resolve(process.cwd(), '../../docs/operations/environment.mdx'), 'utf8')

function uniqueMatches(source: string, pattern: RegExp) {
    return [...new Set([...source.matchAll(pattern)].map((match) => match.groups?.key).filter((key): key is string => key !== undefined))].sort()
}

describe('environment documentation', () => {
    it('documents every operator-facing key used by the Compose file', () => {
        const exampleKeys = uniqueMatches(environmentExample, /^(?<key>VERITAS_[A-Z0-9_]+)=/gm)
        const composeKeys = uniqueMatches(composeFile, /\$\{(?<key>VERITAS_[A-Z0-9_]+)/g)

        expect(exampleKeys).toEqual(composeKeys)
        for (const key of composeKeys) {
            expect(environmentGuide).toContain(`\`${key}\``)
        }
    })
})
