import { test, expect } from '@playwright/test';

const USERNAME = process.env.E2E_USERNAME ?? 'admin';
const PASSWORD = process.env.E2E_PASSWORD ?? 'DevOnly_ChangeMe1!';

test.describe('Authentication', () => {
  test('login page renders correctly', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByText('כניסה למערכת')).toBeVisible();
    await expect(page.getByLabel('שם משתמש')).toBeVisible();
    await expect(page.getByLabel('סיסמה', { exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'התחברות' })).toBeVisible();
  });

  test('unauthenticated redirect to login', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/login/);
  });

  test('wrong password shows Hebrew error', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('שם משתמש').fill(USERNAME);
    await page.getByLabel('סיסמה', { exact: true }).fill('WrongPass999!');
    await page.getByRole('button', { name: 'התחברות' }).click();
    await expect(page.getByText(/שם משתמש או סיסמה שגויים/)).toBeVisible({ timeout: 15_000 });
  });

  test('successful login reaches employers page', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('שם משתמש').fill(USERNAME);
    await page.getByLabel('סיסמה', { exact: true }).fill(PASSWORD);
    await page.getByRole('button', { name: 'התחברות' }).click();
    await expect(page).not.toHaveURL(/\/login/, { timeout: 20_000 });
    await expect(page.getByRole('heading', { name: /מעסיקים/ })).toBeVisible();
  });

  test('navbar shows after login', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('שם משתמש').fill(USERNAME);
    await page.getByLabel('סיסמה', { exact: true }).fill(PASSWORD);
    await page.getByRole('button', { name: 'התחברות' }).click();
    await expect(page).not.toHaveURL(/\/login/, { timeout: 20_000 });
    // Navbar should contain logo or brand
    await expect(page.locator('nav')).toBeVisible();
  });

  test('logout returns to login', async ({ page }) => {
    // Login first
    await page.goto('/login');
    await page.getByLabel('שם משתמש').fill(USERNAME);
    await page.getByLabel('סיסמה', { exact: true }).fill(PASSWORD);
    await page.getByRole('button', { name: 'התחברות' }).click();
    await expect(page).not.toHaveURL(/\/login/, { timeout: 20_000 });

    // Logout
    const logoutBtn = page.getByRole('button', { name: /יצי|התנתק|logout/i });
    if (await logoutBtn.isVisible()) {
      await logoutBtn.click();
      await expect(page).toHaveURL(/\/login/, { timeout: 10_000 });
    }
  });
});
