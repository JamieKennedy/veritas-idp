import rootConfig from '../../prettier.config.cjs'
import * as astroPlugin from 'prettier-plugin-astro'
import * as tailwindPlugin from 'prettier-plugin-tailwindcss'

export default {
    ...rootConfig,
    plugins: [astroPlugin, tailwindPlugin],
    tailwindStylesheet: './src/styles/global.css',
    overrides: [
        ...rootConfig.overrides,
        {
            files: '*.astro',
            options: {
                parser: 'astro',
            },
        },
    ],
}
