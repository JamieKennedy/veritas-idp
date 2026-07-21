// @vitest-environment jsdom

import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { ThemeControl } from './theme-control'
import { ThemeProvider } from './theme-provider'

describe('ThemeControl', () => {
    beforeEach(() => {
        Object.defineProperty(HTMLElement.prototype, 'scrollIntoView', { configurable: true, value: vi.fn() })
        vi.stubGlobal('localStorage', {
            getItem: () => null,
            setItem: vi.fn(),
        })
        vi.stubGlobal(
            'matchMedia',
            vi.fn().mockReturnValue({
                matches: false,
                addEventListener: vi.fn(),
                removeEventListener: vi.fn(),
            }),
        )
    })

    afterEach(() => {
        cleanup()
        vi.unstubAllGlobals()
    })

    it('lets an administrator switch from the system preference to dark mode', () => {
        render(
            <ThemeProvider>
                <ThemeControl />
            </ThemeProvider>,
        )

        fireEvent.click(screen.getByRole('combobox', { name: 'Color theme' }))
        fireEvent.click(screen.getByRole('option', { name: 'Dark' }))

        expect(document.documentElement.classList.contains('dark')).toBe(true)
    })
})
