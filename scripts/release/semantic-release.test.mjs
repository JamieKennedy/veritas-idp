import assert from 'node:assert/strict'
import test from 'node:test'

import releaseConfig from '../../.releaserc.mjs'

test('GitHub releases do not mutate pull requests or issues', () => {
    const githubPlugin = releaseConfig.plugins.find(([pluginName]) => pluginName === '@semantic-release/github')

    assert.ok(githubPlugin, 'The GitHub release plugin must be configured.')

    const [, githubPluginOptions] = githubPlugin
    assert.equal(githubPluginOptions.successCommentCondition, false)
    assert.equal(githubPluginOptions.failCommentCondition, false)
})
