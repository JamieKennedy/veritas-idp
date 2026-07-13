import { readFileSync } from 'node:fs'

import { describe, expect, it } from 'vitest'

import styles from './styles.css?raw'

describe('brand lockup styles', () => {
    it('switches to the reversed dark lockup on the deep navy background', () => {
        const stylesheet = styles || readFileSync(new URL('./styles.css', import.meta.url), 'utf8')

        expect(stylesheet).toMatch(/\.dark\s+\.brand-lockup__light\s*\{\s*display:\s*none;/)
        expect(stylesheet).toMatch(/\.dark\s+\.brand-lockup__dark\s*\{\s*display:\s*block;/)
        expect(stylesheet).toMatch(
            /\.brand-lockup__dark-wordmark\s*\{[^}]*clip-path:\s*inset\(0\s+0\s+0\s+30%\);[^}]*filter:\s*brightness\(0\)\s+invert\(1\);/s,
        )
        expect(stylesheet).toContain('--bg-base: #020817')
    })

    it('uses high-contrast validation colors and a distinct current progress circle', () => {
        const stylesheet = styles || readFileSync(new URL('./styles.css', import.meta.url), 'utf8')

        expect(stylesheet).toContain('--destructive: oklch(0.46 0.19 27)')
        expect(stylesheet).toContain('--destructive: oklch(0.76 0.18 27)')
        expect(stylesheet).toMatch(
            /\.setup-progress__circle\.is-current\s*\{[^}]*border-color:\s*var\(--lagoon\);[^}]*background:\s*#f4f8ff;[^}]*color:\s*#123e92;/s,
        )
    })
})
