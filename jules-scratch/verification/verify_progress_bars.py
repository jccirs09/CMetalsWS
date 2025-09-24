from playwright.sync_api import sync_playwright, expect

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    # Navigate to the application
    page.goto("http://localhost:5186/")
    page.wait_for_load_state()

    # Click login link
    page.get_by_role("link", name="Login").click()
    page.wait_for_url("http://localhost:5186/Account/Login")

    # Login
    page.get_by_label("Email").fill("admin@example.com")
    page.get_by_label("Password").fill("Admin123!")
    page.get_by_role("button", name="Log in").click()
    page.wait_for_load_state()

    # Navigate to Picking & Packing page
    page.get_by_text("Operations").click()
    page.get_by_role("link", name="Picking & Packing").click()
    page.wait_for_url("http://localhost:5186/operations/picking-packing")

    # Screenshot of the page
    page.screenshot(path="jules-scratch/verification/picking_and_packing_with_progress.png")

    browser.close()

with sync_playwright() as playwright:
    run(playwright)