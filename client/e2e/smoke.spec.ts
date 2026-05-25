import { test, expect } from '@playwright/test';

test.describe('SPA shell (no backend required)', () => {
  test('login page shows title', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByText('כניסה למערכת')).toBeVisible();
  });

  test('unauthenticated visit to home redirects to login', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveURL(/\/login/);
  });

  test('login form has username and password fields', async ({ page }) => {
    await page.goto('/login');
    await expect(page.getByLabel('שם משתמש')).toBeVisible();
    await expect(page.getByLabel('סיסמה', { exact: true })).toBeVisible();
    await expect(page.getByRole('button', { name: 'התחברות' })).toBeVisible();
  });
});
