import { readFileSync } from 'node:fs'

import { describe, expect, it } from 'vitest'

import styles from './styles.css?raw'

describe('brand lockup styles', () => {
    it('switches to the reversed dark lockup on the deep navy background', () => {
        const stylesheet = styles || readFileSync(new URL('./styles.css', import.meta.url), 'utf8')

        expect(stylesheet).toContain(`.dark .brand-lockup__light {
    display: none;
}`)
        expect(stylesheet).toContain(`.dark .brand-lockup__dark {
    display: block;
}`)
        expect(stylesheet).toContain(`.brand-lockup__dark-wordmark {
    inset: 0;
    width: 100%;
    height: 100%;
    filter: brightness(0) invert(1);
}`)
        expect(stylesheet).toContain('--bg-base: #020817')
    })

    it('uses high-contrast validation colors and a distinct current progress circle', () => {
        const stylesheet = styles || readFileSync(new URL('./styles.css', import.meta.url), 'utf8')

        expect(stylesheet).toContain('--destructive: oklch(0.46 0.19 27)')
        expect(stylesheet).toContain('--destructive: oklch(0.76 0.18 27)')
        expect(stylesheet).toContain(`.setup-progress__circle.is-current {
    border-color: var(--lagoon);
    background: #f4f8ff;
    color: #123e92;`)
    })
})
