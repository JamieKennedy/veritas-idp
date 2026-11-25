import { useEffect, useState } from 'react'
import { Laptop, Moon, Sun } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from '@/components/ui/tooltip'

type Theme = 'dark' | 'light' | 'system'

const themes: Array<Theme> = ['system', 'light', 'dark']

function applyTheme(theme: Theme) {
    const dark = theme === 'dark' || (theme === 'system' && window.matchMedia('(prefers-color-scheme: dark)').matches)
    document.documentElement.classList.toggle('dark', dark)
    document.documentElement.dataset.theme = theme
}

export function ThemeToggle() {
    const [theme, setTheme] = useState<Theme>('system')

    useEffect(() => {
        const stored = window.localStorage.getItem('veritas-theme')
        const initial = stored === 'dark' || stored === 'light' ? stored : 'system'
        const synchronize = window.setTimeout(() => {
            setTheme(initial)
            applyTheme(initial)
        }, 0)

        const media = window.matchMedia('(prefers-color-scheme: dark)')
        const updateSystemTheme = () => {
            if ((window.localStorage.getItem('veritas-theme') ?? 'system') === 'system') {
                applyTheme('system')
            }
        }
        media.addEventListener('change', updateSystemTheme)
        return () => {
            window.clearTimeout(synchronize)
            media.removeEventListener('change', updateSystemTheme)
        }
    }, [])

    function selectNextTheme() {
        const nextTheme = themes[(themes.indexOf(theme) + 1) % themes.length]
        setTheme(nextTheme)
        window.localStorage.setItem('veritas-theme', nextTheme)
        applyTheme(nextTheme)
    }

    const Icon = theme === 'light' ? Sun : theme === 'dark' ? Moon : Laptop

    return (
        <TooltipProvider delayDuration={300}>
            <Tooltip>
                <TooltipTrigger asChild>
                    <Button type="button" variant="ghost" size="icon" aria-label={`Theme: ${theme}. Change theme.`} onClick={selectNextTheme}>
                        <Icon />
                    </Button>
                </TooltipTrigger>
                <TooltipContent>Theme: {theme}</TooltipContent>
            </Tooltip>
        </TooltipProvider>
    )
}
