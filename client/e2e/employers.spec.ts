import { test, expect, Page } from '@playwright/test';

const USERNAME = process.env.E2E_USERNAME ?? 'admin';
const PASSWORD = process.env.E2E_PASSWORD ?? 'DevOnly_ChangeMe1!';

async function login(page: Page) {
  await page.goto('/login');
  await page.getByLabel('שם משתמש').fill(USERNAME);
  await page.getByLabel('סיסמה', { exact: true }).fill(PASSWORD);
  await page.getByRole('button', { name: 'התחברות' }).click();
  await expect(page).not.toHaveURL(/\/login/, { timeout: 20_000 });
}

test.describe('Employers', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('employers list page loads with table', async ({ page }) => {
    await page.goto('/');
    await expect(page.getByRole('heading', { name: /מעסיקים/ })).toBeVisible();
    // Should have a table or list of employers
    const table = page.locator('table, .employers-list, [data-testid="employers"]');
    await expect(table.first()).toBeVisible({ timeout: 10_000 });
  });

  test('employer names are clickable links', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    const firstLink = page.locator('table td a, .employer-link').first();
    const count = await firstLink.count();
    if (count > 0) {
      await expect(firstLink).toBeVisible();
      const href = await firstLink.getAttribute('href');
      expect(href).toMatch(/\/employers\/\d+/);
    }
  });

  test('navigate to employer details', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    const firstEmployerLink = page.locator('table td a[href*="/employers/"]').first();
    const count = await firstEmployerLink.count();
    if (count > 0) {
      await firstEmployerLink.click();
      await expect(page).toHaveURL(/\/employers\/\d+/, { timeout: 10_000 });
    }
  });

  test('employer detail page shows employee list', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    const firstLink = page.locator('table td a[href*="/employers/"]').first();
    const count = await firstLink.count();
    if (count > 0) {
      await firstLink.click();
      await page.waitForLoadState('networkidle');
      // Should show employees table or empty state
      const content = page.locator('table, .empty-state, [class*="employee"]');
      await expect(content.first()).toBeVisible({ timeout: 10_000 });
    }
  });

  test('employer detail page has actions button', async ({ page }) => {
    await page.goto('/');
    await page.waitForLoadState('networkidle');
    const firstLink = page.locator('table td a[href*="/employers/"]').first();
    const count = await firstLink.count();
    if (count > 0) {
      await firstLink.click();
      await page.waitForLoadState('networkidle');
      // Check for actions button (rendered as a button, not a link)
      const actionsBtn = page.getByRole('button', { name: /פעולות ודוחות/ });
      await expect(actionsBtn).toBeVisible({ timeout: 5_000 });
    }
  });
});
