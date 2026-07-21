import { pathToFileURL } from 'node:url'

export function validateManualRelease({ refName, confirmed }) {
    if (refName !== 'staging') {
        throw new Error("Manual prereleases must run against the 'staging' branch.")
    }

    if (String(confirmed).toLowerCase() !== 'true') {
        throw new Error('Manual prereleases require explicit confirmation.')
    }
}

if (process.argv[1] && import.meta.url === pathToFileURL(process.argv[1]).href) {
    try {
        validateManualRelease({
            refName: process.env.GITHUB_REF_NAME,
            confirmed: process.env.RELEASE_CONFIRMED,
        })
        console.log(`Manual staging release confirmed for ${process.env.GITHUB_SHA ?? 'the selected staging commit'}.`)
    } catch (error) {
        console.error(`::error::${error.message}`)
        process.exitCode = 1
    }
}
