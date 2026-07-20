// @vitest-environment jsdom

import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import veritasLockupUrl from '@brand/veritas-lockup.svg?url'
import veritasMarkUrl from '@brand/veritas-mark.svg?url'
import { BrandLockup } from './brand-lockup'

describe('BrandLockup', () => {
    it('renders one accessible name across light and dark presentations with both shared brand assets', () => {
        const { container } = render(<BrandLockup className="test-lockup" />)

        const lightPresentation = container.querySelector('.brand-lockup__light')
        const darkPresentation = container.querySelector('.brand-lockup__dark')
        const darkWordmark = container.querySelector('.brand-lockup__dark-wordmark')
        const darkMark = container.querySelector('.brand-lockup__dark-mark')
        const lockup = screen.getByRole('img', { name: 'Veritas' })

        expect(screen.getAllByRole('img', { name: 'Veritas' })).toHaveLength(1)
        expect(lockup.classList.contains('brand-lockup')).toBe(true)
        expect(lockup.classList.contains('test-lockup')).toBe(true)
        expect(lightPresentation).not.toBeNull()
        expect(lightPresentation?.getAttribute('src')).toBe(veritasLockupUrl)
        expect(darkPresentation).not.toBeNull()
        expect(darkWordmark?.getAttribute('src')).toBe(veritasLockupUrl)
        expect(darkMark?.getAttribute('src')).toBe(veritasMarkUrl)
    })
})
