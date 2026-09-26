import { Page } from '@playwright/test';

export class AuthPage {
  readonly page: Page;
  readonly url = 'http://localhost:5074/Auth/Index';

  constructor(page: Page) {
    this.page = page;
  }

  async goto() {
    await this.page.goto(this.url);
  }

  get usernameInput() {
    return this.page.locator('input[name="Username"], #loginUsername');
  }

  get passwordInput() {
    return this.page.locator('input[name="Password"], #loginPassword');
  }

  get loginButton() {
    return this.page.locator('button[type="submit"]');
  }

  async login(username: string, password: string = '1') {
    await this.usernameInput.fill(username);
    await this.passwordInput.fill(password);
    await this.loginButton.click();
    await this.page.waitForURL(/.*(Dashboard|Home).*/);
  }
}
