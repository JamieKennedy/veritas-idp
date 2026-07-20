import assert from 'node:assert/strict'
import { execFileSync } from 'node:child_process'
import { readFileSync } from 'node:fs'
import path from 'node:path'

const repositoryRoot = path.resolve(import.meta.dirname, '../..')
const generatedRouteTree = path.join(repositoryRoot, 'src/frontend/admin-ui/src/routeTree.gen.ts')
const before = readFileSync(generatedRouteTree, 'utf8')
const pnpmCli = process.env.npm_execpath

assert.ok(pnpmCli, 'This check must run through pnpm so the pinned package manager is used.')
execFileSync(process.execPath, [pnpmCli, 'generate:routes'], {
    cwd: repositoryRoot,
    stdio: 'inherit',
})

const after = readFileSync(generatedRouteTree, 'utf8')
assert.equal(after, before, 'Generated routes are stale. Run pnpm generate:routes and commit the result.')

console.log('Verified generated TanStack routes.')
