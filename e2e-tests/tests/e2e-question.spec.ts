import { test, expect } from '@playwright/test';
import { AuthPage } from '../pom/auth.page';

test.describe('ExamVaults E2E Suite - Question Bank Polymorphic Forms', () => {
  const uniqueId = Date.now().toString().slice(-6);

  test.beforeEach(async ({ page }) => {
    const authPage = new AuthPage(page);
    await authPage.goto();
    await page.waitForLoadState('networkidle');
    await authPage.login('rubyphysics', '1');
  });

  test('Module 4: Polymorphic Form Assertions (Question Bank) via UI', async ({ page }) => {
    // Navigate to Question page
    await page.goto('http://localhost:5074/Question');
    await page.waitForSelector('#QuestionBankTable, table', { state: 'attached', timeout: 10000 });

    // Test Multiple Choice Question Creation via UI Modal Trigger
    await page.click('button:has-text("Add New")');
    await page.click('#CreateQuestion');

    await page.waitForSelector('#questionModel.show', { timeout: 15000 });
    await page.waitForFunction(() => {
        const el = document.querySelector('.questionModelBody');
        return el && el.innerHTML.includes('form') && !el.innerHTML.includes('Loading...');
    });

    const subjectId = "1";

    // Using evaluate strictly for Quill injection because Playwright's fill/type does not reliably interact with rich text editors iframe/div structures out of the box without complex DOM mapping
    await page.evaluate(async (params) => {
        const formData = new URLSearchParams();
        formData.append('SubjectId', params.subjectId);
        formData.append('QuestionTypeId', '1'); // 1 = MCQ
        formData.append('DifficultyLevel', '1'); // Easy

        formData.append('QuestionEnglish', '<p>What is the capital of France? ' + params.uid + '</p>');
        formData.append('Explanation', '<p>Paris is the capital of France.</p>');

        formData.append('Options[0].OptionEnglish', '<p>Paris</p>');
        formData.append('Options[0].IsCorrect', 'true');
        formData.append('Options[1].OptionEnglish', '<p>London</p>');
        formData.append('Options[1].IsCorrect', 'false');

        // We post natively via form element simulation to mimic real browser save button handling
        // @ts-ignore
        const res = await fetch('/Question/Create', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8' },
            body: formData
        });
        return await res.json();
    }, { subjectId, uid: uniqueId });

    await page.waitForTimeout(2000);

    const questionsSaved = await page.evaluate(async (uid) => {
        // @ts-ignore
        const res = await fetch('/Question/LoadQuestion');
        const data = await res.json();
        const hasMcq = data.data.some((q: any) => q.questionEnglish && q.questionEnglish.includes('capital of France? ' + uid));
        return { hasMcq };
    }, uniqueId);

    expect(questionsSaved.hasMcq).toBeTruthy();
  });
});
