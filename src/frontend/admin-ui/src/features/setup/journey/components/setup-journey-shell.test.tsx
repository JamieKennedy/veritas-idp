// @vitest-environment jsdom

import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { ThemeProvider } from '@/app/theme/theme-provider'
import { SetupJourneyShell } from './setup-journey-shell'

describe('SetupJourneyShell', () => {
    beforeEach(() => {
        vi.stubGlobal('localStorage', { getItem: () => null, setItem: vi.fn() })
        vi.stubGlobal(
            'matchMedia',
            vi.fn().mockReturnValue({
                matches: false,
                addEventListener: vi.fn(),
                removeEventListener: vi.fn(),
            }),
        )
    })

    it('places the brand, active progress, theme control, and page content in the setup journey', () => {
        render(
            <ThemeProvider>
                <SetupJourneyShell currentStage="secure">
                    <h1>Secure the first administrator</h1>
                </SetupJourneyShell>
            </ThemeProvider>,
        )

        expect(screen.getByText('Veritas')).not.toBeNull()
        expect(screen.getByText('Current step: Secure administrator')).not.toBeNull()
        expect(screen.getByRole('combobox', { name: 'Color theme' })).not.toBeNull()
        expect(screen.getByRole('heading', { name: 'Secure the first administrator' })).not.toBeNull()
    })
})
