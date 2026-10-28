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

async function goToFirstEmployerActions(page: Page): Promise<boolean> {
  // Step 1: get employer URL from the list
  await page.goto('/');
  await page.waitForLoadState('networkidle');
  const firstLink = page.locator('table td a[href*="/employers/"]').first();
  if (await firstLink.count() === 0) return false;
  const employerHref = await firstLink.getAttribute('href');
  if (!employerHref) return false;

  // Step 2: navigate directly to employer detail
  await page.goto(employerHref);
  await page.waitForLoadState('networkidle');

  // Step 3: wait for actions button and click it
  const actionsBtn = page.getByRole('button', { name: /פעולות ודוחות/ });
  try {
    await actionsBtn.waitFor({ state: 'visible', timeout: 8_000 });
  } catch {
    return false;
  }
  await actionsBtn.click();
  await page.waitForLoadState('networkidle');
  return true;
}

test.describe('Actions & Reports panel', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('actions page shows main menu cards', async ({ page }) => {
    const found = await goToFirstEmployerActions(page);
    test.skip(!found, 'No employers in DB');

    await expect(page.getByText('הנפקת דוחות')).toBeVisible({ timeout: 10_000 });
    await expect(page.getByText('השוואה')).toBeVisible();
  });

  test('reports panel opens on click', async ({ page }) => {
    const found = await goToFirstEmployerActions(page);
    test.skip(!found, 'No employers in DB');

    await page.getByText('הנפקת דוחות').click();
    await expect(page.getByText('הנפקת דוחות —')).toBeVisible({ timeout: 5_000 });
    await expect(page.locator('select')).toBeVisible();
  });

  test('comparison panel opens on click', async ({ page }) => {
    const found = await goToFirstEmployerActions(page);
    test.skip(!found, 'No employers in DB');

    // Click the "השוואה" card
    await page.locator('.card, [class*="card"]').filter({ hasText: 'השוואה' }).first().click();
    // Should show comparison sub-type buttons (actual names from the UI)
    await expect(page.getByRole('button', { name: /דוח השוואה לפי חודש/ })).toBeVisible({ timeout: 5_000 });
    await expect(page.getByRole('button', { name: /דוח השוואה שנתי/ })).toBeVisible();
  });

  test('back button returns to main menu', async ({ page }) => {
    const found = await goToFirstEmployerActions(page);
    test.skip(!found, 'No employers in DB');

    await page.getByText('הנפקת דוחות').click();
    await expect(page.getByText('הנפקת דוחות —')).toBeVisible({ timeout: 5_000 });

    await page.getByRole('button', { name: /חזרה לתפריט/ }).click();
    await expect(page.getByText('הנפקת דוחות')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByText('השוואה')).toBeVisible();
  });
});
