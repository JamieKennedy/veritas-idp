import { describe, expect, it } from 'vitest'

import { landingContent } from './landing'

describe('landingContent', () => {
    it('is an original typed source with complete developer calls to action', () => {
        expect(landingContent.title).toContain('identity layer')
        expect(landingContent.description).toContain('self-hostable')
        expect(landingContent.links.documentation.href).toBe('/docs/')
        expect(landingContent.links.install.href).toBe('/operations/docker-compose/')
        expect(landingContent.links.contribute.href).toContain('CONTRIBUTING.md')
        expect(landingContent.install.commands.bash).toContain('cp .env.example .env')
        expect(landingContent.install.commands.powershell).toContain('Copy-Item .env.example .env')
    })

    it('states the maturity boundary without implying missing protocol support', () => {
        expect(landingContent.warning).toContain('pre-alpha')
        expect(landingContent.warning).toContain('must not be used')
        expect(landingContent.roadmap.some((item) => item.title.includes('OpenID Connect'))).toBe(true)
        expect(landingContent.available.every((item) => item.title.length > 0 && item.description.length > 0)).toBe(true)
    })
})
