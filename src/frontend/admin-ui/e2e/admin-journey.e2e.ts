import { createHmac, randomBytes } from 'node:crypto'

import { expect, test } from '@playwright/test'

test('an administrator completes setup, enrolls MFA, and can sign out and back in', async ({ page }) => {
    const bootstrapSecret = process.env.VERITAS_TEST_BOOTSTRAP_SECRET
    if (!bootstrapSecret) {
        throw new Error('Start the disposable stack with pnpm check:admin-workflow.')
    }

    const email = 'first-admin@example.test'
    const password = `Verifier-${randomBytes(20).toString('hex')}!`

    const initialPage = await page.goto('/', { waitUntil: 'networkidle' })
    expect(initialPage?.status()).toBe(200)
    await page.getByLabel('First administrator email').fill(email)
    await page.getByLabel('Deployment bootstrap secret').fill(bootstrapSecret)
    const csrfIssued = page.waitForResponse((response) => new URL(response.url()).pathname === '/api/v1/admin-auth/csrf')
    const bootstrapStarted = page.waitForResponse((response) => new URL(response.url()).pathname === '/api/v1/bootstrap/start')
    await page.getByRole('button', { name: 'Verify and continue' }).click()
    expect((await csrfIssued).status()).toBe(200)
    expect((await bootstrapStarted).status()).toBe(202)
    await page.getByLabel('Password', { exact: true }).fill(password)
    await page.getByLabel('Confirm password').fill(password)
    await page.getByRole('button', { name: 'Create administrator' }).click()

    const signIn = async () => {
        await page.getByLabel('Administrator email').fill(email)
        await page.getByLabel('Password', { exact: true }).fill(password)
        await page.getByRole('button', { name: 'Continue', exact: true }).click()
    }

    await signIn()
    const secret = await page.locator('code').first().innerText()
    await page.getByLabel('Verification code').fill(totp(secret))
    await page.getByRole('button', { name: 'Verify and enroll' }).click()
    await expect(page.getByText('These recovery codes are shown once.', { exact: false })).toBeVisible()
    const recoveryCode = await page.locator('form ul li').first().innerText()
    await page.getByRole('checkbox', { name: 'I have stored these recovery codes securely.' }).check()
    await page.getByRole('button', { name: 'Continue to Veritas' }).click()

    await page.getByRole('button', { name: 'Set up later' }).click()
    await page.getByRole('button', { name: 'Skip email setup' }).click()
    await expect(page).toHaveURL(/\/dashboard$/)

    await page.getByRole('button', { name: 'Sign out' }).click()
    await expect(page).toHaveURL((url) => url.pathname === '/login')
    const signedOut = await page.request.get('/api/v1/admin-auth/me')
    expect(signedOut.status()).toBe(401)

    await signIn()
    await page.getByLabel('Authenticator or recovery code').fill(recoveryCode)
    await page.getByRole('button', { name: 'Sign in' }).click()
    await expect(page).toHaveURL(/\/dashboard$/)
})

function totp(secret: string) {
    const alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ234567'
    const bits = Array.from(secret, (character) => alphabet.indexOf(character).toString(2).padStart(5, '0')).join('')
    const key = Buffer.from(bits.match(/.{8}/g)?.map((byte) => Number.parseInt(byte, 2)) ?? [])
    const counter = Buffer.alloc(8)
    counter.writeBigUInt64BE(BigInt(Math.floor(Date.now() / 30_000)))
    const digest = createHmac('sha1', key).update(counter).digest()
    const offset = digest.readUInt8(digest.length - 1) & 15
    return ((digest.readUInt32BE(offset) & 0x7fffffff) % 1_000_000).toString().padStart(6, '0')
}
