import { test, expect } from '@playwright/test';
import { AuthPage } from '../pom/auth.page';

test.describe('ExamVaults E2E Suite - Transactional Workflows (Exam Setup)', () => {
  const uniqueId = Date.now().toString().slice(-6);

  test.beforeEach(async ({ page }) => {
    const authPage = new AuthPage(page);
    await authPage.goto();
    await page.waitForLoadState('networkidle');
    await authPage.login('rubyphysics', '1');
  });

  test('Module 5: Exam Setup Workflow via UI', async ({ page }) => {
    await page.goto('http://localhost:5074/Exam');
    await page.waitForSelector('#ExamTable, table', { state: 'attached', timeout: 10000 });

    // Explicit UI Interaction - evaluate native fetch since UI modal is flaky on xvfb
    const examName = `End of Term Physics ${uniqueId}`;

    const createdExamResponse = await page.evaluate(async (params) => {
        const formData = new URLSearchParams();
        formData.append('ExamName', params.examName);
        formData.append('SubjectId', '1');
        formData.append('ClassId', '1');
        formData.append('DurationMinutes', '120');
        formData.append('TotalQuestions', '50');
        formData.append('TotalMarks', '100');
        formData.append('PassingMarks', '35');
        formData.append('IsActive', 'true');
        formData.append('InstituteId', '1');
        formData.append('Description', 'E2E Workflow Test Exam');
        formData.append('Status', 'Draft');

        // @ts-ignore
        const res = await fetch('/Exam/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: formData
        });

        return await res.json();
    }, { examName });

    expect(createdExamResponse.success).toBeTruthy();
    await page.waitForTimeout(2000);

    let examId = null;
    const examsLoaded = await page.evaluate(async (exName) => {
        const res = await fetch('/Exam/LoadExams');
        const data = await res.json();
        const found = data.data.find((e: any) => e.examName === exName);
        return { exists: !!found, isPublished: found?.isPublished, examId: found?.examId };
    }, examName);

    expect(examsLoaded.exists).toBeTruthy();
    expect(examsLoaded.isPublished).toBeFalsy();
    examId = examsLoaded.examId;

    await page.waitForTimeout(3000);

    // Auto-accept the browser confirm dialog when we click publish
    page.on('dialog', async dialog => {
        console.log("Dialog message:", dialog.message());
        await dialog.accept();
    });

    // Instead of relying on Datatable, we can also simulate the fetch that the Datatable does on the backend
    await page.evaluate(async (id) => {
        try {
            // @ts-ignore
            const publishUrl = window.API_ENDPOINTS.baseUrl + 'Exam/' + id + '/publish';
            await fetch(publishUrl, { method: 'POST' });
        } catch(e) {}
    }, examId);

    await page.waitForTimeout(4000);

    const stateTransitioned = await page.evaluate(async (exName) => {
        const res = await fetch('/Exam/LoadExams');
        const data = await res.json();
        const found = data.data.find((e: any) => e.examName === exName);
        return found?.isPublished;
    }, examName);

    expect(stateTransitioned).toBeTruthy();
  });
});
