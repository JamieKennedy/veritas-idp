import { createRequire } from 'node:module'
import { pathToFileURL } from 'node:url'

const rootRequire = createRequire(import.meta.url)
const adminUiRequire = createRequire(new URL('./src/frontend/admin-ui/package.json', import.meta.url))

function resolveWorkspacePackage(packageName) {
    try {
        return rootRequire.resolve(packageName)
    } catch {
        return adminUiRequire.resolve(packageName)
    }
}

async function importWorkspacePackage(packageName) {
    return import(pathToFileURL(resolveWorkspacePackage(packageName)).href)
}

const js = (await importWorkspacePackage('@eslint/js')).default
const { tanstackConfig } = await importWorkspacePackage('@tanstack/eslint-config')
const query = (await importWorkspacePackage('@tanstack/eslint-plugin-query')).default
const router = (await importWorkspacePackage('@tanstack/eslint-plugin-router')).default
const prettier = (await importWorkspacePackage('eslint-config-prettier')).default
const reactHooks = (await importWorkspacePackage('eslint-plugin-react-hooks')).default
const reactRefresh = (await importWorkspacePackage('eslint-plugin-react-refresh')).default
const globals = (await importWorkspacePackage('globals')).default
const tseslint = (await importWorkspacePackage('typescript-eslint')).default

export default [
    {
        ignores: [
            '**/eslint.config.*',
            '**/prettier.config.*',
            '**/dist/**',
            '**/.output/**',
            '**/node_modules/**',
            '**/src/routeTree.gen.ts',
            '**/src/shared/api/generated/schema.ts',
        ],
    },
    ...tanstackConfig,
    {
        plugins: {
            '@tanstack/query': query,
            '@tanstack/router': router,
            'react-hooks': reactHooks,
            'react-refresh': reactRefresh,
        },
        rules: {
            'import/no-cycle': 'off',
            'import/order': 'off',
            'sort-imports': 'off',
            '@typescript-eslint/array-type': 'off',
            '@typescript-eslint/require-await': 'off',
            'pnpm/json-enforce-catalog': 'off',
            ...reactHooks.configs.recommended.rules,
            '@tanstack/query/exhaustive-deps': 'error',
            '@tanstack/router/create-route-property-order': 'error',
            '@typescript-eslint/consistent-type-definitions': 'off',
            '@typescript-eslint/no-confusing-void-expression': 'off',
            'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
        },
    },
    js.configs.recommended,
    ...tseslint.configs.strictTypeChecked,
    ...tseslint.configs.stylisticTypeChecked,
    {
        ...tseslint.configs.disableTypeChecked,
        files: ['src/frontend/**/server/**/*.mjs'],
        languageOptions: {
            ...tseslint.configs.disableTypeChecked.languageOptions,
            globals: globals.node,
        },
    },
    prettier,
]
