import assert from 'node:assert/strict'
import { readFileSync } from 'node:fs'

const eventPath = process.env.GITHUB_EVENT_PATH
assert.ok(eventPath, 'GITHUB_EVENT_PATH is required.')

const event = JSON.parse(readFileSync(eventPath, 'utf8'))
const pullRequest = event.pull_request
assert.ok(pullRequest, 'This check must run for a pull request event.')

const baseBranch = pullRequest.base.ref
const headBranch = pullRequest.head.ref
const title = pullRequest.title
const body = pullRequest.body ?? ''

const branchPattern = /^(?:feature|bugfix|chore|docs|hotfix)\/[a-z0-9]+(?:[a-z0-9._-]*[a-z0-9])?$/
const dependencyBranchPattern = /^dependabot\//
const titlePattern = /^(?:feat|fix|perf|refactor|docs|test|chore|ci|build|revert)(?:\([a-z0-9._/-]+\))?!?: .+/

assert.ok(
    branchPattern.test(headBranch) || dependencyBranchPattern.test(headBranch) || headBranch === 'staging',
    `Branch '${headBranch}' does not follow the documented branch naming convention.`,
)
assert.match(title, titlePattern, 'The PR title must follow Conventional Commits.')

if (baseBranch === 'main') {
    assert.ok(headBranch === 'staging' || headBranch.startsWith('hotfix/'), 'Only staging or hotfix/* may target main.')
}

if (baseBranch === 'main' && headBranch === 'staging') {
    assert.match(body, /Tested prerelease:\s*v?\d+\.\d+\.\d+-beta\.\d+/i, "A staging promotion must include 'Tested prerelease: x.y.z-beta.n'.")
    assert.match(body, /-\s*\[[xX]\]\s*Manual Compose staging test completed/, "A staging promotion must check 'Manual Compose staging test completed'.")
}

console.log(`Validated ${headBranch} -> ${baseBranch}: ${title}`)
