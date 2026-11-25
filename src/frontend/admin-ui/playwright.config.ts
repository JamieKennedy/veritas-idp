import { defineConfig } from '@playwright/test'

export default defineConfig({
    testDir: './e2e',
    testMatch: '**/*.e2e.ts',
    use: { baseURL: process.env.VERITAS_TEST_BASE_URL, ignoreHTTPSErrors: true },
})
