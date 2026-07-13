// @vitest-environment jsdom

import { cleanup, fireEvent, render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { ThemeProvider, themeStorageKey, useTheme } from './theme-provider'

function ThemeProbe() {
    const { preference, resolvedTheme, setPreference } = useTheme()

    return (
        <>
            <p>{`${preference}:${resolvedTheme}`}</p>
            <button
                type="button"
                onClick={() => {
                    setPreference('light')
                }}
            >
                Use light
            </button>
        </>
    )
}

describe('ThemeProvider', () => {
    beforeEach(() => {
        const storedValues = new Map<string, string>()
        vi.stubGlobal('localStorage', {
            clear: () => {
                storedValues.clear()
            },
            getItem: (key: string) => storedValues.get(key) ?? null,
            removeItem: (key: string) => storedValues.delete(key),
            setItem: (key: string, value: string) => storedValues.set(key, value),
        })
        document.documentElement.className = ''
        document.documentElement.style.colorScheme = ''
        vi.stubGlobal(
            'matchMedia',
            vi.fn().mockReturnValue({
                matches: true,
                addEventListener: vi.fn(),
                removeEventListener: vi.fn(),
            }),
        )
    })

    afterEach(() => {
        cleanup()
        vi.unstubAllGlobals()
    })

    it('defaults to the system preference and applies its resolved dark theme to the document', () => {
        render(
            <ThemeProvider>
                <ThemeProbe />
            </ThemeProvider>,
        )

        expect(screen.getByText('system:dark')).not.toBeNull()
        expect(document.documentElement.classList.contains('dark')).toBe(true)
        expect(document.documentElement.style.colorScheme).toBe('dark')
    })

    it('persists an explicit selection and applies it instead of the system preference', () => {
        render(
            <ThemeProvider>
                <ThemeProbe />
            </ThemeProvider>,
        )

        fireEvent.click(screen.getByRole('button', { name: 'Use light' }))

        expect(screen.getByText('light:light')).not.toBeNull()
        expect(localStorage.getItem(themeStorageKey)).toBe('light')
        expect(document.documentElement.classList.contains('dark')).toBe(false)
        expect(document.documentElement.style.colorScheme).toBe('light')
    })

    it('uses light mode when system preference detection is unavailable', () => {
        vi.stubGlobal('matchMedia', undefined)

        render(
            <ThemeProvider>
                <ThemeProbe />
            </ThemeProvider>,
        )

        expect(screen.getByText('system:light')).not.toBeNull()
        expect(document.documentElement.classList.contains('dark')).toBe(false)
    })
})
