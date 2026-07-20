export function resolveReleaseImages({ githubRepository, registry = 'ghcr.io' }) {
    const repositoryParts = githubRepository?.split('/') ?? []
    if (repositoryParts.length !== 2 || repositoryParts.some((part) => part.length === 0)) {
        throw new Error('GITHUB_REPOSITORY must use the owner/repository format.')
    }

    const repositoryOwner = repositoryParts[0].toLowerCase()

    return {
        adminUi: `${registry}/${repositoryOwner}/veritas-admin-ui`,
        backend: `${registry}/${repositoryOwner}/veritas-backend`,
    }
}
