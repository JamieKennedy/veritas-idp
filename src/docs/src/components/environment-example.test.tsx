import { readFileSync } from 'node:fs'
import path from 'node:path'

import { fireEvent, render, screen, waitFor } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { EnvironmentExample } from './environment-example'

const environmentExample = readFileSync(path.resolve(process.cwd(), '../../.env.example'), 'utf8')

const operatorEnvironmentKeys = [
    'VERITAS_VERSION',
    'VERITAS_BIND_ADDRESS',
    'VERITAS_ADMIN_UI_PORT',
    'VERITAS_POSTGRES_PASSWORD',
    'VERITAS_RABBITMQ_PASSWORD',
    'VERITAS_BOOTSTRAP_SECRET_FILE',
]

describe('EnvironmentExample', () => {
    const writeText = vi.fn<() => Promise<void>>()

    beforeEach(() => {
        writeText.mockReset()
        writeText.mockResolvedValue()
        Object.defineProperty(navigator, 'clipboard', {
            configurable: true,
            value: { writeText },
        })
    })

    it('renders every operator-facing environment key from the tracked example', () => {
        render(<EnvironmentExample source={environmentExample} />)

        const example = screen.getByLabelText('Complete Veritas environment file example')
        for (const key of operatorEnvironmentKeys) {
            expect(environmentExample).toContain(`${key}=`)
            expect(example.textContent).toContain(`${key}=`)
        }
    })

    it('copies the complete tracked example and announces success', async () => {
        render(<EnvironmentExample source={environmentExample} />)

        fireEvent.click(screen.getByRole('button', { name: 'Copy' }))

        await waitFor(() => {
            expect(writeText).toHaveBeenCalledWith(environmentExample)
        })
        expect(screen.getByRole('status').textContent).toContain('Environment example copied to the clipboard.')
        expect(screen.getByRole('button', { name: 'Copied' })).not.toBeNull()
    })

    it('announces a clipboard failure without throwing', async () => {
        writeText.mockRejectedValueOnce(new Error('Clipboard unavailable'))
        render(<EnvironmentExample source={environmentExample} />)

        fireEvent.click(screen.getByRole('button', { name: 'Copy' }))

        await waitFor(() => {
            expect(screen.getByRole('status').textContent).toContain('could not be copied')
        })
        expect(screen.getByRole('button', { name: 'Copy failed' })).not.toBeNull()
    })
})
