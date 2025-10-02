from playwright.sync_api import sync_playwright, expect

def run_verification(playwright):
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    try:
        # Navigate to the Destination Regions page
        page.goto("http://localhost:5186/admin/destination-regions")
        page.wait_for_selector('text="Destination Regions"')

        # Take a screenshot of the Destination Regions page
        page.screenshot(path="jules-scratch/verification/destination-regions.png")

        # Navigate to the Customers page
        page.goto("http://localhost:5186/customers")
        page.wait_for_selector('text="Customers"')

        # Take a screenshot of the Customers page
        page.screenshot(path="jules-scratch/verification/customers.png")

        # Navigate to the Picking and Packing page
        page.goto("http://localhost:5186/operations/picking-packing")
        page.wait_for_selector('text="Pick and Pack"')

        # Take a screenshot of the Picking and Packing page
        page.screenshot(path="jules-scratch/verification/picking-packing.png")

    finally:
        browser.close()

with sync_playwright() as playwright:
    run_verification(playwright)