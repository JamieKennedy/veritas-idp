// @ts-check

const fs = require('node:fs')
const path = require('node:path')

const tailwindPluginCandidates = [
    path.join(__dirname, 'node_modules/prettier-plugin-tailwindcss/dist/index.mjs'),
    path.join(__dirname, 'src/frontend/admin-ui/node_modules/prettier-plugin-tailwindcss/dist/index.mjs'),
]

const tailwindPluginPath = tailwindPluginCandidates.find((candidate) => fs.existsSync(candidate))

/** @type {import('prettier').Config} */
const config = {
    plugins: tailwindPluginPath === undefined ? [] : [tailwindPluginPath],
    semi: false,
    singleQuote: true,
    trailingComma: 'all',
    tabWidth: 4,
    printWidth: 160,
    overrides: [
        {
            files: ['*.json', '*.jsonc', '*.yaml', '*.yml'],
            options: {
                tabWidth: 2,
            },
        },
    ],
}

module.exports = config
