from playwright.sync_api import sync_playwright

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    # Log in
    page.goto("http://localhost:5068/account/login")
    page.get_by_label("Email").fill("admin@example.com")
    page.get_by_label("Password").fill("Admin123!")
    page.get_by_role("button", name="Log in").click()
    page.wait_for_url("http://localhost:5068/")

    # Navigate to Pick and Pack page
    page.goto("http://localhost:5068/operations/picking-packing")

    # Initiate a picking list if any is pending
    initiate_button = page.get_by_role("button", name="Initiate Process").first
    if initiate_button.is_visible():
        initiate_button.click()
        page.wait_for_load_state("networkidle")

    # Go to Picking Process tab
    page.get_by_role("tab", name="Picking Process").click()
    page.wait_for_load_state("networkidle")

    # Complete a pick
    start_pick_button = page.get_by_role("button", name="Start Pick").first
    if start_pick_button.is_visible():
        start_pick_button.click()
        page.wait_for_load_state("networkidle")

        complete_button = page.get_by_role("button", name="Complete").first
        if complete_button.is_visible():
            complete_button.click()
            page.get_by_role("button", name="Confirm").click()
            page.wait_for_load_state("networkidle")

    # Go to Packing Process tab
    page.get_by_role("tab", name="Packing Process").click()
    page.wait_for_load_state("networkidle")

    # Complete a pack
    start_pack_button = page.get_by_role("button", name="Start Pack").first
    if start_pack_button.is_visible():
        start_pack_button.click()
        page.wait_for_load_state("networkidle")

        complete_button = page.get_by_role("button", name="Complete").first
        if complete_button.is_visible():
            complete_button.click()
            # Fill out the dialog
            dialog = page.get_by_role("dialog")
            dialog.get_by_label("Quantity").fill("1")
            dialog.get_by_label("Weight (lbs)").fill("10")
            dialog.get_by_label("Notes").fill("Test notes")
            dialog.get_by_role("button", name="Confirm").click()
            page.wait_for_load_state("networkidle")


    # Take a screenshot
    page.screenshot(path="jules-scratch/verification/picking_and_packing.png")

    browser.close()

with sync_playwright() as playwright:
    run(playwright)