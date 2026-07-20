import { execFileSync } from 'node:child_process'
import path from 'node:path'

const repositoryRoot = path.resolve(import.meta.dirname, '../..')
const gitOutput = execFileSync('git', ['ls-files', '--cached', '--others', '--exclude-standard', '--', '*.md'], {
    cwd: repositoryRoot,
    encoding: 'utf8',
})
const markdownFiles = gitOutput
    .split(/\r?\n/)
    .map((line) => line.trim())
    .filter(Boolean)

if (markdownFiles.length === 0) {
    console.log('No Markdown files found.')
    process.exit(0)
}

const markdownlintCli = path.join(repositoryRoot, 'node_modules/markdownlint-cli2/markdownlint-cli2-bin.mjs')
execFileSync(process.execPath, [markdownlintCli, ...markdownFiles], {
    cwd: repositoryRoot,
    stdio: 'inherit',
})
