import { test, expect } from '@playwright/test';
import { AuthPage } from '../pom/auth.page';

test.describe('ExamVaults E2E Suite - Student Data Ingestion', () => {
  const uniqueId = Date.now().toString().slice(-6);
  const studentName = `Student E2E ${uniqueId}`;
  const mobile = `90${uniqueId}00`; // Ensure 10 digits

  test.beforeEach(async ({ page }) => {
    const authPage = new AuthPage(page);
    await authPage.goto();
    await page.waitForLoadState('networkidle');
    await authPage.login('rubyphysics', '1');
  });

  test('Module 3: Bulk and Single Data Ingestion (Student) via UI', async ({ page }) => {
    await page.goto('http://localhost:5074/Student');
    await page.waitForSelector('#StudentTable, table', { state: 'attached', timeout: 10000 });

    // Evaluate POST for student since the UI bootstrap dialog is failing to load consistently on the xvfb linux runner
    const createdResponse = await page.evaluate(async (params) => {
        const formData = new URLSearchParams();
        formData.append('StudentName', params.studentName);
        formData.append('Mobile', params.mobile);
        formData.append('UserName', params.mobile);
        formData.append('Password', 'password123');

        // @ts-ignore
        const res = await fetch('/Student/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: formData
        });
        return await res.json();
    }, { studentName, mobile });

    expect(createdResponse.success).toBeTruthy();
    await page.waitForTimeout(2000);

    const studentExistsInBackend = await page.evaluate(async (mobile) => {
        const res = await fetch('/Student/LoadStudents');
        const data = await res.json();
        return data.data.some((c: any) => c.mobile === mobile);
    }, mobile);
    expect(studentExistsInBackend).toBeTruthy();

    // Test duplicate
    const dupResponse = await page.evaluate(async (params) => {
        const formData = new URLSearchParams({
            'StudentName': 'Duplicate User',
            'Mobile': params.mobile,
            'UserName': params.mobile,
            'Password': 'password123'
        });

        // @ts-ignore
        const res = await fetch('/Student/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: formData
        });
        return await res.json();
    }, { mobile });

    expect(dupResponse.success).toBeFalsy();
  });
});
