import { Monitor, Moon, Sun } from 'lucide-react'

import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { ThemeProvider, useTheme } from '@/app/theme/theme-provider'
import { normalizeThemePreference } from '@/app/theme/theme'
import type { ThemePreference } from '@/app/theme/theme'

const themeOptions: ReadonlyArray<{ value: ThemePreference; label: string; icon: typeof Monitor }> = [
    { value: 'system', label: 'System', icon: Monitor },
    { value: 'light', label: 'Light', icon: Sun },
    { value: 'dark', label: 'Dark', icon: Moon },
]

export function ThemeControl() {
    return (
        <ThemeProvider>
            <ThemeSelector />
        </ThemeProvider>
    )
}

function ThemeSelector() {
    const { preference, setPreference } = useTheme()

    return (
        <Select
            value={preference}
            onValueChange={(value) => {
                setPreference(normalizeThemePreference(value))
            }}
        >
            <SelectTrigger size="sm" aria-label="Color theme">
                <SelectValue />
            </SelectTrigger>
            <SelectContent align="end">
                {themeOptions.map(({ value, label, icon: Icon }) => (
                    <SelectItem key={value} value={value}>
                        <Icon /> {label}
                    </SelectItem>
                ))}
            </SelectContent>
        </Select>
    )
}
