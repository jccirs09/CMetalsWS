from playwright.sync_api import sync_playwright, expect

def run(playwright):
    browser = playwright.chromium.launch(headless=True)
    context = browser.new_context()
    page = context.new_page()

    try:
        # 1. Log in to the application
        page.goto("http://localhost:5186/Account/Login")
        page.get_by_label("Email").fill("admin@example.com")
        page.get_by_label("Password").fill("Admin123!")
        page.get_by_role("button", name="Log in").click()
        expect(page).to_have_url("http://localhost:5186/")

        # 2. Open the messaging system
        page.get_by_role("button", name="Chat").click()
        messaging_system = page.locator(".messaging-system-container")
        expect(messaging_system).to_be_visible()
        page.screenshot(path="jules-scratch/verification/01_messaging_system_open.png")

        # 3. Select a contact and view the conversation
        contacts_tab = page.get_by_role("tab", name="Contacts")
        contacts_tab.click()

        # Click the 'user1' contact
        page.get_by_role("listitem").filter(has_text="user1").click()

        conversation_view = page.locator(".conversation-view")
        expect(conversation_view).to_contain_text("user1")
        page.screenshot(path="jules-scratch/verification/02_conversation_view.png")

        # 4. Send a message
        message_input = page.get_by_placeholder("Type a message...")
        message_input.fill("Hello from the verification script!")
        page.get_by_label("Send").click()

        # Wait for the message to appear
        expect(page.locator(".message-item").last).to_contain_text("Hello from the verification script!")
        page.screenshot(path="jules-scratch/verification/03_message_sent.png")

    finally:
        browser.close()

with sync_playwright() as playwright:
    run(playwright)