// @vitest-environment jsdom

import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { SetupProgress } from './setup-progress'

describe('SetupProgress', () => {
    it('marks earlier stages complete and identifies the active stage', () => {
        render(<SetupProgress currentStage="email" />)

        expect(screen.getByText('Complete: Claim installation')).not.toBeNull()
        expect(screen.getByText('Complete: Secure administrator')).not.toBeNull()
        expect(screen.getByText('Current step: Configure email')).not.toBeNull()
        expect(screen.getByText('Configure email').closest('li')?.getAttribute('aria-current')).toBe('step')
    })
})
