import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { InstallPreview } from './install-preview'

const commands = {
    bash: ['cp .env.example .env', 'docker compose up -d'],
    powershell: ['Copy-Item .env.example .env', 'docker compose up -d'],
}

describe('InstallPreview', () => {
    const writeText = vi.fn<() => Promise<void>>()

    beforeEach(() => {
        writeText.mockReset()
        writeText.mockResolvedValue()
        Object.defineProperty(navigator, 'clipboard', {
            configurable: true,
            value: { writeText },
        })
    })

    it('switches between Bash and PowerShell commands', () => {
        render(<InstallPreview commands={commands} description="Install Veritas." />)

        expect(screen.getByLabelText('Bash Docker Compose installation preview')).not.toBeNull()
        fireEvent.mouseDown(screen.getByRole('tab', { name: 'PowerShell' }), { button: 0, ctrlKey: false })
        expect(screen.getByLabelText('PowerShell Docker Compose installation preview').textContent).toContain('Copy-Item')
    })

    it('copies commands from the active shell', async () => {
        render(<InstallPreview commands={commands} description="Install Veritas." />)

        fireEvent.mouseDown(screen.getByRole('tab', { name: 'PowerShell' }), { button: 0, ctrlKey: false })
        fireEvent.click(screen.getByRole('button', { name: 'Copy PowerShell installation commands' }))

        await waitFor(() => {
            expect(writeText).toHaveBeenCalledWith(commands.powershell.join('\n'))
        })
        expect(screen.getByRole('status').textContent).toContain('PowerShell installation commands copied')
    })
})
