import { describe, expect, it } from 'vitest'

import { normalizeThemePreference, resolveTheme } from './theme'

describe('theme preferences', () => {
    it('uses the operating system preference when the saved preference is system', () => {
        expect(resolveTheme('system', true)).toBe('dark')
        expect(resolveTheme('system', false)).toBe('light')
    })

    it('uses an explicit light or dark preference instead of the operating system preference', () => {
        expect(resolveTheme('light', true)).toBe('light')
        expect(resolveTheme('dark', false)).toBe('dark')
    })

    it('falls back to system when persisted data is absent or invalid', () => {
        expect(normalizeThemePreference(null)).toBe('system')
        expect(normalizeThemePreference('dim')).toBe('system')
    })
})
