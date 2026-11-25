import { cleanup, fireEvent, render, waitFor, within } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

import { CodeBlockControls } from './CodeBlockControls'

describe('CodeBlockControls', () => {
    const writeText = vi.fn<() => Promise<void>>()

    beforeEach(() => {
        writeText.mockReset()
        writeText.mockResolvedValue()
        Object.defineProperty(navigator, 'clipboard', {
            configurable: true,
            value: { writeText },
        })
    })

    afterEach(() => {
        cleanup()
        document.body.innerHTML = ''
    })

    it('groups equivalent shells into keyboard-accessible tabs', async () => {
        document.body.innerHTML = `
            <article class="prose">
                <div data-shell-tabs="install"></div>
                <pre data-language="bash"><code>cp .env.example .env</code></pre>
                <pre data-language="powershell"><code>Copy-Item .env.example .env</code></pre>
            </article>
        `

        render(<CodeBlockControls />)

        const tablist = await waitFor(() => within(document.body).getByRole('tablist', { name: 'Shell' }))
        const bash = within(tablist).getByRole('tab', { name: 'Bash' })
        const powershell = within(tablist).getByRole('tab', { name: 'PowerShell' })

        expect(bash.getAttribute('aria-selected')).toBe('true')
        expect(document.getElementById(powershell.getAttribute('aria-controls') ?? '')?.hidden).toBe(true)

        fireEvent.keyDown(bash, { key: 'ArrowRight' })

        expect(powershell.getAttribute('aria-selected')).toBe('true')
        expect(document.activeElement).toBe(powershell)
    })

    it('copies the active shell and adds copy controls to ordinary code', async () => {
        document.body.innerHTML = `
            <article class="prose">
                <div data-shell-tabs="install"></div>
                <pre data-language="bash"><code>docker compose up -d</code></pre>
                <pre data-language="powershell"><code>docker compose up -d --wait</code></pre>
                <pre data-language="json"><code>{"enabled":true}</code></pre>
            </article>
        `

        render(<CodeBlockControls />)

        const blocks = await waitFor(() => document.querySelectorAll<HTMLElement>('.code-block'))
        expect(blocks).toHaveLength(2)

        const shellBlock = blocks[0]
        fireEvent.click(within(shellBlock).getByRole('tab', { name: 'PowerShell' }))
        fireEvent.click(within(shellBlock).getByRole('button', { name: 'Copy code' }))

        await waitFor(() => {
            expect(writeText).toHaveBeenCalledWith('docker compose up -d --wait')
        })
        expect(within(shellBlock).getByText('Code copied to clipboard')).not.toBeNull()

        const jsonBlock = blocks[1]
        expect(within(jsonBlock).getByText('JSON')).not.toBeNull()
        expect(within(jsonBlock).getByRole('button', { name: 'Copy code' })).not.toBeNull()
    })
})
