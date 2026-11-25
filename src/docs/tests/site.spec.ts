import { expect, test } from '@playwright/test'
import type { Page } from '@playwright/test'

async function waitForHydration(page: Page, componentExport: string) {
    await expect(page.locator(`astro-island[component-export="${componentExport}"]`)).not.toHaveAttribute('ssr', '')
}

test('presents the developer landing page and maturity boundary', async ({ page }) => {
    await page.goto('./')

    await expect(
        page.getByRole('heading', {
            level: 1,
            name: /own the identity layer/i,
        }),
    ).toBeVisible()
    await expect(page.getByLabel('Software maturity warning')).toContainText('Pre-alpha')
    await expect(page.getByRole('link', { name: 'Read the documentation' }).first()).toHaveAttribute('href', '/veritas-idp/docs/')
    await waitForHydration(page, 'LandingCapabilities')
    await expect(page.getByText('OAuth 2.0 and OpenID Connect')).toBeHidden()
    await page.getByRole('tab', { name: 'Still being built' }).click()
    await expect(page.getByText('OAuth 2.0 and OpenID Connect')).toBeVisible()

    const installPreview = page.locator('astro-island[component-export="InstallPreview"]')
    await installPreview.scrollIntoViewIfNeeded()
    await waitForHydration(page, 'InstallPreview')
    const installShells = installPreview.getByRole('tablist', { name: 'Installation shell' })
    await installShells.getByRole('tab', { name: 'PowerShell' }).click()
    await expect(installPreview.getByLabel('PowerShell Docker Compose installation preview')).toContainText('Copy-Item .env.example .env')
})

test('renders stable documentation routes and local navigation', async ({ page }) => {
    await page.goto('./operations/environment/')

    await expect(page.getByRole('heading', { level: 1, name: 'Environment variables' })).toBeVisible()
    await expect(page.getByLabel('Complete Veritas environment file example')).toContainText('VERITAS_POSTGRES_PASSWORD')
    await expect(page.getByRole('link', { name: 'Secret management' }).first()).toHaveAttribute('href', '/veritas-idp/operations/secrets/')
})

test('switches shell examples and exposes copy controls for every snippet', async ({ page }) => {
    await page.goto('./operations/docker-compose/')
    await waitForHydration(page, 'CodeBlockControls')

    const shellTabs = page.getByRole('tablist', { name: 'Shell' }).first()
    await expect(shellTabs.getByRole('tab', { name: 'Bash' })).toHaveAttribute('aria-selected', 'true')
    await shellTabs.getByRole('tab', { name: 'PowerShell' }).click()
    await expect(shellTabs.getByRole('tab', { name: 'PowerShell' })).toHaveAttribute('aria-selected', 'true')

    const codeBlocks = page.locator('.prose .code-block')
    await expect(codeBlocks.first().getByRole('button', { name: 'Copy code' })).toBeVisible()
    await expect(page.locator('.prose .code-block .code-toolbar__copy')).toHaveCount(await codeBlocks.count())
})

test('searches the production Pagefind index from the keyboard', async ({ page }) => {
    await page.goto('./docs/')
    await waitForHydration(page, 'SearchDialog')
    await page.keyboard.press(process.platform === 'darwin' ? 'Meta+K' : 'Control+K')
    await page.getByPlaceholder('Search deployment, secrets, MFA…').fill('bootstrap')

    await expect(page.getByRole('option').first()).toBeVisible()
})

test('supports theme selection and mobile documentation navigation', async ({ page }, testInfo) => {
    await page.goto('./docs/')

    await waitForHydration(page, 'ThemeToggle')
    const theme = page.getByRole('button', { name: /theme:/i })
    await theme.click()
    await expect(page.locator('html')).toHaveAttribute('data-theme', 'light')

    if (testInfo.project.name === 'mobile-chromium') {
        await waitForHydration(page, 'MobileNavigation')
        await page.getByRole('button', { name: 'Open documentation navigation' }).click()
        await expect(page.getByRole('dialog', { name: 'Veritas documentation' })).toBeVisible()
        await expect(page.getByRole('link', { name: 'Docker Compose' })).toBeVisible()
    }
})

test('serves the branded not-found page', async ({ page }) => {
    const response = await page.goto('./route-that-does-not-exist/')

    expect(response?.status()).toBe(404)
    await expect(
        page.getByRole('heading', {
            level: 1,
            name: 'This route has no identity.',
        }),
    ).toBeVisible()
})
