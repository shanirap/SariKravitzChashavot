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

/** Navigate to the first employee details page that exists */
async function goToFirstEmployee(page: Page): Promise<boolean> {
  // Step 1: find the first employer URL
  await page.goto('/');
  await page.waitForLoadState('networkidle');
  const firstEmployerLink = page.locator('table td a[href*="/employers/"]').first();
  if (await firstEmployerLink.count() === 0) return false;
  const employerHref = await firstEmployerLink.getAttribute('href');
  if (!employerHref) return false;

  // Step 2: navigate to employer detail and wait for employee table
  await page.goto(employerHref);
  await page.waitForLoadState('networkidle');
  // Wait for employee table rows to appear
  const employeeLinks = page.locator('table td a[href*="/employees/"]');
  try {
    await employeeLinks.first().waitFor({ state: 'visible', timeout: 10_000 });
  } catch {
    return false;
  }
  // Prefer an employee that already has employment data (no ?addEmployment=1 in href)
  const allEmployeeLinks = await employeeLinks.all();
  let chosenHref: string | null = null;
  for (const link of allEmployeeLinks) {
    const href = await link.getAttribute('href');
    if (href && !href.includes('addEmployment')) {
      chosenHref = href;
      break;
    }
  }
  // Fallback: take the first link and strip the query param
  if (!chosenHref) {
    const fallbackHref = await employeeLinks.first().getAttribute('href');
    if (!fallbackHref) return false;
    chosenHref = fallbackHref.split('?')[0];
  }

  // Step 3: navigate to employee detail page
  await page.goto(chosenHref);
  await page.waitForLoadState('networkidle');
  return true;
}

test.describe('Employment Data — Accordion & Multi-Year', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('employment data section is visible', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB for this test');

    // Use the page heading which is unique
    await expect(page.getByRole('heading', { name: /נתוני העסקה/ })).toBeVisible({ timeout: 10_000 });
  });

  test('existing records show year badge but not expanded content initially', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const yearBadge = page.locator('.badge-month').first();
    const count = await yearBadge.count();
    if (count === 0) return; // No records — skip

    await expect(yearBadge).toBeVisible();
    // Content (דרגה 1) should be hidden initially
    const recordContent = page.locator('.employment-records-detail .border-top').first();
    await expect(recordContent).toBeHidden();
  });

  test('clicking year row expands employment data', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const yearBadge = page.locator('.badge-month').first();
    const count = await yearBadge.count();
    if (count === 0) return;

    // Click the accordion header
    await yearBadge.click();

    // Now content should be visible
    const expandedContent = page.locator('.employment-records-detail .border-top').first();
    await expect(expandedContent).toBeVisible({ timeout: 3_000 });
  });

  test('clicking expanded row collapses it', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const yearBadge = page.locator('.badge-month').first();
    const count = await yearBadge.count();
    if (count === 0) return;

    // Open
    await yearBadge.click();
    const expandedContent = page.locator('.employment-records-detail .border-top').first();
    await expect(expandedContent).toBeVisible({ timeout: 3_000 });

    // Close
    await yearBadge.click();
    await expect(expandedContent).toBeHidden({ timeout: 3_000 });
  });

  test('add-next-year button is visible when records exist', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const addYearBtn = page.getByRole('button', { name: /הוסף שנה/ });
    const count = await page.locator('.badge-month').count();
    if (count === 0) return;

    await expect(addYearBtn).toBeVisible({ timeout: 5_000 });
  });

  test('add-next-year button opens pre-filled form', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const addYearBtn = page.getByRole('button', { name: /הוסף שנה/ });
    const count = await page.locator('.badge-month').count();
    if (count === 0) return;

    await addYearBtn.click();

    // Form should appear with year label
    await expect(page.getByText(/הוספת שנת לימודים/)).toBeVisible({ timeout: 5_000 });
    // Cancel button should appear (there may be more than one, just check at least one is visible)
    await expect(page.getByRole('button', { name: /ביטול/ }).first()).toBeVisible();
  });

  test('cancel closes the add-next-year form', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const addYearBtn = page.getByRole('button', { name: /הוסף שנה/ });
    const count = await page.locator('.badge-month').count();
    if (count === 0) return;

    await addYearBtn.click();
    await expect(page.getByText(/הוספת שנת לימודים/)).toBeVisible({ timeout: 5_000 });

    // Cancel
    await page.getByRole('button', { name: /ביטול/ }).first().click();
    await expect(page.getByText(/הוספת שנת לימודים/)).toBeHidden({ timeout: 3_000 });
    // Add year button should be back
    await expect(page.getByRole('button', { name: /הוסף שנה/ })).toBeVisible();
  });
});

test.describe('Employment Data — Edit modal', () => {
  test.beforeEach(async ({ page }) => {
    await login(page);
  });

  test('edit button opens modal', async ({ page }) => {
    const found = await goToFirstEmployee(page);
    test.skip(!found, 'No employees in DB');

    const count = await page.locator('.badge-month').count();
    if (count === 0) return;

    // Click pencil icon
    const editBtn = page.locator('button[title="עריכה"]').first();
    await expect(editBtn).toBeVisible();
    await editBtn.click();

    await expect(page.locator('#editModal')).toBeVisible({ timeout: 5_000 });
    await expect(page.getByText('עריכת נתוני העסקה')).toBeVisible();
  });
});
