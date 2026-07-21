import assert from 'node:assert/strict'
import { execFileSync } from 'node:child_process'
import { readdirSync, readFileSync } from 'node:fs'
import path from 'node:path'

const repositoryRoot = path.resolve(import.meta.dirname, '../..')
const requiredFrontendScripts = ['build', 'format:check', 'lint', 'test', 'typecheck']

const trackedProjects = gitLines(['ls-files', 'src/**/*.csproj']).map(normalizePath)
const solutionProjects = execFileSync('dotnet', ['sln', 'Veritas.slnx', 'list'], {
    cwd: repositoryRoot,
    encoding: 'utf8',
})
    .split(/\r?\n/)
    .map((line) => normalizePath(line.trim()))
    .filter((line) => line.endsWith('.csproj'))

assert.deepEqual([...solutionProjects].sort(), [...trackedProjects].sort(), 'Every tracked .NET project under src must be registered in Veritas.slnx.')

const frontendRoot = path.join(repositoryRoot, 'src/frontend')
const frontendPackages = readdirSync(frontendRoot, { withFileTypes: true })
    .filter((entry) => entry.isDirectory())
    .map((entry) => path.join(frontendRoot, entry.name, 'package.json'))
    .filter((packagePath) => gitLines(['ls-files', normalizePath(path.relative(repositoryRoot, packagePath))]).length > 0)

for (const packagePath of frontendPackages) {
    const manifest = JSON.parse(readFileSync(packagePath, 'utf8'))
    const relativePath = normalizePath(path.relative(repositoryRoot, packagePath))

    assert.equal(manifest.private, true, `${relativePath} must remain a private workspace package.`)
    for (const scriptName of requiredFrontendScripts) {
        assert.equal(typeof manifest.scripts?.[scriptName], 'string', `${relativePath} must define the '${scriptName}' script.`)
    }
}

const unexpectedLockfiles = gitLines(['ls-files', '**/package-lock.json', '**/yarn.lock'])
assert.deepEqual(unexpectedLockfiles, [], 'pnpm-lock.yaml is the only permitted JavaScript lockfile.')

const rootManifest = JSON.parse(readFileSync(path.join(repositoryRoot, 'package.json'), 'utf8'))
assert.equal(rootManifest.packageManager, 'pnpm@10.15.1', 'The repository must pin pnpm 10.15.1.')
assert.equal(rootManifest.engines?.node, '>=22.12.0 <23', 'The repository must remain on the supported Node 22 line.')

console.log(`Verified ${trackedProjects.length} .NET projects and ${frontendPackages.length} frontend package(s).`)

function gitLines(arguments_) {
    return execFileSync('git', arguments_, {
        cwd: repositoryRoot,
        encoding: 'utf8',
    })
        .split(/\r?\n/)
        .map((line) => line.trim())
        .filter(Boolean)
}

function normalizePath(value) {
    return value.replaceAll('\\', '/')
}
