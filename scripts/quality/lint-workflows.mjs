import { execFileSync } from 'node:child_process'

const actionlintImage = 'rhysd/actionlint:1.7.7@sha256:887a259a5a534f3c4f36cb02dca341673c6089431057242cdc931e9f133147e9'

execFileSync('docker', ['run', '--rm', '--volume', `${process.cwd()}:/workspace:ro`, '--workdir', '/workspace', actionlintImage], {
    stdio: 'inherit',
})
