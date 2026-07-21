import assert from 'node:assert/strict'
import test from 'node:test'

import { validateManualRelease } from './validate-manual-release.mjs'

test('accepts an explicitly confirmed staging release', () => {
    assert.doesNotThrow(() => validateManualRelease({ refName: 'staging', confirmed: 'true' }))
})

test('rejects a manual release from another branch', () => {
    assert.throws(() => validateManualRelease({ refName: 'main', confirmed: 'true' }), /must run against the 'staging' branch/)
})

test('rejects an unconfirmed manual release', () => {
    assert.throws(() => validateManualRelease({ refName: 'staging', confirmed: 'false' }), /require explicit confirmation/)
})
