import { readFileSync } from 'node:fs'

import { describe, expect, it } from 'vitest'

import styles from './styles.css?raw'

describe('brand lockup styles', () => {
    it('constrains the setup lockup and gives it a light neutral plate', () => {
        const stylesheet = styles || readFileSync(new URL('./styles.css', import.meta.url), 'utf8')

        expect(stylesheet).toContain('.setup-journey__lockup')
        expect(stylesheet).toContain('width: clamp(10rem, 22vw, 13rem)')
        expect(stylesheet).toContain('background: #f8fbff')
    })
})
