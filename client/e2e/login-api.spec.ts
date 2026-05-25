import { test, expect } from '@playwright/test';

const apiBase = process.env.E2E_API_URL ?? 'http://localhost:5036';

test.describe('Login vs live API', () => {
  test.beforeEach(async ({ request }) => {
    const live = (await request.get(`${apiBase}/swagger/index.html`)).ok();
    if (!live) {
      test.skip(true, `API not reachable at ${apiBase} — start the server (dotnet run) for this test.`);
    }
  });

  test('wrong password shows Hebrew error', async ({ page }) => {
    await page.goto('/login');
    await page.getByLabel('שם משתמש').fill(process.env.E2E_USERNAME ?? 'admin');
    await page.getByLabel('סיסמה', { exact: true }).fill('WrongPass999!');
    await page.getByRole('button', { name: 'התחברות' }).click();
    await expect(page.getByText(/שם משתמש או סיסמה שגויים/)).toBeVisible({
      timeout: 15_000,
    });
  });

  test('happy path reaches employers after login', async ({ page }) => {
    const password = process.env.E2E_PASSWORD;
    test.skip(!password, 'Set E2E_PASSWORD (and optionally E2E_USERNAME) for full login E2E.');
    const username = process.env.E2E_USERNAME ?? 'admin';

    await page.goto('/login');
    await page.getByLabel('שם משתמש').fill(username);
    await page.getByLabel('סיסמה', { exact: true }).fill(password!);
    await page.getByRole('button', { name: 'התחברות' }).click();

    await expect(page).not.toHaveURL(/\/login/, { timeout: 20_000 });
    await expect(page.getByRole('link', { name: /מעסיקים/ })).toBeVisible();
  });
});
