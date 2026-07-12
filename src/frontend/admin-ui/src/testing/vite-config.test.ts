import type { ConfigEnv, Plugin, PluginOption, UserConfig } from 'vite'
import { describe, expect, it } from 'vitest'

import viteConfig from '../../vite.config'

describe('Admin UI Vite configuration', () => {
    it('disables TanStack Devtools console piping to prevent recursive log forwarding', () => {
        const plugins = flattenPlugins(viteConfig.plugins ?? [])
        const consolePipe = plugins.find((plugin) => plugin.name === '@tanstack/devtools:console-pipe-transform')

        expect(consolePipe).toBeDefined()
        expect(typeof consolePipe?.apply).toBe('function')

        const applies = (consolePipe?.apply as (config: UserConfig, env: ConfigEnv) => boolean)(
            { mode: 'development' },
            { command: 'serve', mode: 'development', isSsrBuild: false, isPreview: false },
        )

        expect(applies).toBe(false)
    })
})

function flattenPlugins(options: Array<PluginOption>): Array<Plugin> {
    const flattened = (options as Array<unknown>).flat(Infinity)

    return flattened.filter(
        (option): option is Plugin => typeof option === 'object' && option !== null && 'name' in option,
    )
}
