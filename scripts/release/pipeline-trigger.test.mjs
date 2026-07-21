import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'
import test from 'node:test'

const pipeline = readFileSync('.github/workflows/pipeline.yml', 'utf8')
const triggers = pipeline.slice(pipeline.indexOf('on:'), pipeline.indexOf('\npermissions:'))
const pushTrigger = triggers.slice(triggers.indexOf('  push:'), triggers.indexOf('  workflow_dispatch:'))

test('staging releases use manual dispatch instead of a push trigger', () => {
    assert.match(pushTrigger, /- main/)
    assert.doesNotMatch(pushTrigger, /- staging/)
    assert.match(triggers, /workflow_dispatch:/)
    assert.match(triggers, /confirm_staging_release:/)
})

test('release job distinguishes manual staging from automatic production releases', () => {
    assert.match(pipeline, /github\.event_name == 'push' && github\.ref_name == 'main'/)
    assert.match(pipeline, /github\.event_name == 'workflow_dispatch' && github\.ref_name == 'staging'/)
})
