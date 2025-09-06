# CMetalsWS

This is a Blazor-based web application for CMetals.

## Getting Started

### Prerequisites

-   **.NET 9 SDK**: The project is built on .NET 9.
-   **Docker**: A local development database is provided via Docker. Ensure Docker Desktop is installed and running.
-   **Poppler**: The PDF parsing feature requires the `pdftoimage` utility, which is part of the Poppler toolset.
    -   **Windows**: Download Poppler for Windows, extract it, and add the `bin` directory to your system's PATH.
    -   **macOS (Homebrew)**: `brew install poppler`
    -   **Linux (Debian/Ubuntu)**: `sudo apt-get install poppler-utils`
    -   Alternatively, you can specify the full path to the executable in the `appsettings.Development.json` file.

### Configuration

1.  **Database Connection**: The application is configured to connect to a SQL Server instance running in a Docker container. The default connection string in `appsettings.json` should work with the provided `docker-compose.yml` file.

2.  **User Secrets**: This project uses User Secrets to store sensitive information like API keys. Initialize user secrets for the project:
    ```bash
    dotnet user-secrets init
    ```

3.  **OpenAI API Key**: The PDF parsing feature uses the OpenAI Vision API. You need to provide an API key.
    ```bash
    dotnet user-secrets set "OpenAI:ApiKey" "YOUR_OPENAI_API_KEY"
    ```

4.  **Poppler Path (Optional)**: If you did not add Poppler to your system's PATH, you can specify the location of the executable in `appsettings.Development.json`:
    ```json
    "PdfToImage": {
      "ExecutablePath": "C:\\path\\to\\poppler\\bin\\pdftoppm.exe",
      "Dpi": 300
    }
    ```

### Running the Application

1.  **Start the database**:
    ```bash
    sudo docker compose up -d
    ```

2.  **Run the application**:
    ```bash
    dotnet run
    ```

The application will be available at `https://localhost:5001` (or another port specified in the launch settings).

## PDF Parsing Feature

This application includes a feature to upload, parse, and review picking list PDFs.

### How to Test

1.  Navigate to the **Picking Lists** page from the main menu.
2.  Click the **Upload PDF** button.
3.  On the upload page:
    -   Select a **Branch**.
    -   Choose a sample picking list PDF to upload (sample files are available in `Samples/PickingLists/`).
    -   Click **Upload and Parse**.
4.  The application will process the PDF, which may take a few moments. You will see status updates on the screen.
5.  Upon completion, you will be redirected to the **Review** page for the newly imported picking list.
6.  On the review page, you can assign a **Machine** to each line item using the dropdowns. You can also select multiple rows and use the **Bulk Assign** tool.
7.  Click **Save** or **Save & Close** to persist your machine assignments.
8.  Clicking **Re-Parse** will re-run the AI parsing process on the original PDF, updating the data while preserving existing machine assignments for matching lines.
