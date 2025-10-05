from playwright.sync_api import sync_playwright, expect

def run_verification():
    with sync_playwright() as p:
        browser = p.chromium.launch(headless=True)
        page = browser.new_page()

        try:
            print("Navigating to the Unified Load Planning page...")
            page.goto("http://localhost:5186/planning/unified-load-planning", timeout=90000)

            print("Waiting for the main heading to be visible...")
            expect(page.get_by_role("heading", name="Loads & Shipping")).to_be_visible(timeout=30000)

            print("Finding and clicking the 'Customer Pickup' region card...")
            # Refined locator to be more specific by looking for a card that has a heading with the exact text.
            pickup_region_card = page.locator(".mud-card", has=page.get_by_role("heading", name="Customer Pickup", exact=True))
            expect(pickup_region_card).to_be_visible(timeout=15000)
            pickup_region_card.click()

            print("Verifying that the 'Pickup Management' tab is active...")
            pickup_management_tab = page.get_by_role("tab", name="Pickup Management")
            # The tab should be active after the click
            expect(pickup_management_tab).to_have_attribute("aria-selected", "true", timeout=10000)

            print("Finding the 'Record Pickup' button for the first order...")
            # The button is inside the CustomerPickupManagement component
            record_pickup_button = page.get_by_role("button", name="Record Pickup").first
            expect(record_pickup_button).to_be_visible(timeout=10000)
            record_pickup_button.click()

            print("Verifying that the 'Record Pickup' dialog is visible...")
            dialog_title = page.get_by_role("heading", name="Record Pickup for")
            expect(dialog_title).to_be_visible(timeout=10000)

            print("Verification successful. Capturing screenshot...")
            page.screenshot(path="jules-scratch/verification/pickup_dialog_verification.png")

        except Exception as e:
            print(f"An error occurred during verification: {e}")
            page.screenshot(path="jules-scratch/verification/error.png")

        finally:
            browser.close()

run_verification()