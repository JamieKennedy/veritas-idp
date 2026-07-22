import fs from 'node:fs'
import path from 'node:path'

const repositoryRoot = path.resolve(import.meta.dirname, '../..')
const expectedLicense = 'AGPL-3.0-only'
const failures = []

function read(relativePath) {
    return fs.readFileSync(path.join(repositoryRoot, relativePath), 'utf8')
}

function readJson(relativePath) {
    return JSON.parse(read(relativePath))
}

for (const manifest of ['package.json', 'src/frontend/admin-ui/package.json']) {
    if (readJson(manifest).license !== expectedLicense) {
        failures.push(`${manifest} must declare ${expectedLicense}`)
    }
}

const buildProperties = read('Directory.Build.props')
for (const expected of [
    '<PackageLicenseExpression>AGPL-3.0-only</PackageLicenseExpression>',
    '<RepositoryUrl>https://github.com/JamieKennedy/veritas-idp</RepositoryUrl>',
]) {
    if (!buildProperties.includes(expected)) {
        failures.push(`Directory.Build.props is missing ${expected}`)
    }
}

if (!read('LICENSE').includes('GNU AFFERO GENERAL PUBLIC LICENSE')) {
    failures.push('LICENSE must contain the GNU Affero General Public License')
}

const readme = read('README.md')
if (!readme.includes('pre-alpha') || !readme.includes('[GNU Affero General Public License v3.0 only](LICENSE)')) {
    failures.push('README.md must state the maturity warning and AGPL-3.0-only license')
}

for (const requiredFile of ['SECURITY.md', 'SUPPORT.md', 'CONTRIBUTING.md', 'CODE_OF_CONDUCT.md', 'docs/security/threat-model.md']) {
    if (!fs.existsSync(path.join(repositoryRoot, requiredFile))) {
        failures.push(`${requiredFile} is required for public readiness`)
    }
}

if (failures.length > 0) {
    console.error(['Public metadata validation failed:', ...failures.map((failure) => `- ${failure}`)].join('\n'))
    process.exit(1)
}

console.log('Public license, maturity, repository, and community metadata are consistent.')
