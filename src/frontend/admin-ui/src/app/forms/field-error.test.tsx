// @vitest-environment jsdom

import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'

import { FieldError } from './field-error'

describe('FieldError', () => {
    it('announces the first validation message with the supplied id', () => {
        render(<FieldError id="email-error" errors={['Enter a valid email.']} />)

        const error = screen.getByRole('alert')
        expect(error.id).toBe('email-error')
        expect(error.textContent).toBe('Enter a valid email.')
    })
})
