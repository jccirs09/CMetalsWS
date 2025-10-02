import re
import time
from playwright.sync_api import Page, expect, sync_playwright, Error

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    # Give the server a moment to start
    time.sleep(5)

    try:
        # Log in
        page.goto("http://localhost:5018/Account/Login", timeout=10000)
        page.get_by_label("Email").fill("admin@example.com")
        page.get_by_label("Password").fill("Admin123!")
        page.get_by_role("button", name="Log in").click()
        expect(page).to_have_url("http://localhost:5018/", timeout=10000)

        # Navigate to Destination Regions
        page.goto("http://localhost:5018/admin/destination-regions")

        # Create a new region
        page.get_by_role("button", name="New Region").click()

        # Fill in the dialog
        dialog = page.get_by_role("dialog")
        expect(dialog).to_be_visible()

        dialog.get_by_label("Name").fill("Test Region")
        dialog.get_by_label("Icon").fill("fas fa-star")
        dialog.get_by_label("Color").fill("#ff00ff")
        dialog.get_by_role("button", name="Save").click()

        # Verify the new region in the admin table
        expect(page.get_by_role("cell", name="Test Region")).to_be_visible()

        # Navigate to Unified Load Planning
        page.goto("http://localhost:5018/planning/unified-load-planning")

        # Verify the new region card is displayed with the correct styling
        region_card = page.get_by_role("heading", name="Test Region").locator("..").locator("..")
        expect(region_card).to_be_visible()

        avatar = region_card.get_by_role("img")
        expect(avatar).to_have_attribute("style", "background-color: #ff00ff;")

        # Take a screenshot
        page.screenshot(path="jules-scratch/verification/verification.png")

    except Error as e:
        print(f"An error occurred during Playwright execution: {e}")
        page.screenshot(path="jules-scratch/verification/error.png")

    finally:
        browser.close()

with sync_playwright() as playwright:
    run(playwright)