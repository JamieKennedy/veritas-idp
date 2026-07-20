/* eslint-disable react-refresh/only-export-components -- Theme provider exports its hook and storage key as part of the same UI contract. */
import { createContext, useContext, useEffect, useState } from 'react'

import { normalizeThemePreference, resolveTheme } from './theme'
import type { ResolvedTheme, ThemePreference } from './theme'

export const themeStorageKey = 'veritas.admin.theme'

interface ThemeContextValue {
    preference: ThemePreference
    resolvedTheme: ResolvedTheme
    setPreference: (preference: ThemePreference) => void
}

const ThemeContext = createContext<ThemeContextValue | undefined>(undefined)

export function ThemeProvider({ children }: { children: React.ReactNode }) {
    const [preference, setPreferenceState] = useState(readStoredThemePreference)
    const [systemPrefersDark, setSystemPrefersDark] = useState(getSystemPrefersDark)
    const resolvedTheme = resolveTheme(preference, systemPrefersDark)

    useEffect(() => {
        document.documentElement.classList.toggle('dark', resolvedTheme === 'dark')
        document.documentElement.style.colorScheme = resolvedTheme
    }, [resolvedTheme])

    useEffect(() => {
        const mediaQuery = getSystemColorSchemeMediaQuery()
        if (!mediaQuery) {
            return undefined
        }

        const handleChange = (event: MediaQueryListEvent) => {
            setSystemPrefersDark(event.matches)
        }

        mediaQuery.addEventListener('change', handleChange)
        return () => {
            mediaQuery.removeEventListener('change', handleChange)
        }
    }, [])

    useEffect(() => {
        const handleStorage = (event: StorageEvent) => {
            if (event.key === themeStorageKey) {
                setPreferenceState(normalizeThemePreference(event.newValue))
            }
        }

        window.addEventListener('storage', handleStorage)
        return () => {
            window.removeEventListener('storage', handleStorage)
        }
    }, [])

    function setPreference(nextPreference: ThemePreference) {
        setPreferenceState(nextPreference)
        try {
            localStorage.setItem(themeStorageKey, nextPreference)
        } catch {
            // Theme preferences are non-essential and must not prevent rendering when storage is unavailable.
        }
    }

    return <ThemeContext.Provider value={{ preference, resolvedTheme, setPreference }}>{children}</ThemeContext.Provider>
}

export function useTheme(): ThemeContextValue {
    const value = useContext(ThemeContext)
    if (!value) {
        throw new Error('useTheme must be used within ThemeProvider.')
    }

    return value
}

function readStoredThemePreference(): ThemePreference {
    try {
        return normalizeThemePreference(localStorage.getItem(themeStorageKey))
    } catch {
        return 'system'
    }
}

function getSystemPrefersDark() {
    return getSystemColorSchemeMediaQuery()?.matches ?? false
}

function getSystemColorSchemeMediaQuery() {
    if (typeof window === 'undefined') {
        return undefined
    }

    const browserWindow = window as unknown as { matchMedia?: (query: string) => MediaQueryList }
    return browserWindow.matchMedia?.('(prefers-color-scheme: dark)')
}
