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

### PDF Parsing Design

The PDF parsing pipeline is designed to be robust and to run in an intranet environment without requiring external image hosting.

-   **Intranet-Safe Design**: All images generated from the PDF are handled locally. They are converted into `data:image/jpeg;base64,...` URLs and sent directly to the OpenAI Vision API. This avoids the need to upload images to a public server (like Azure Blob Storage) and then pass a public URL, ensuring data remains within the request body.

-   **Payload Size Management**: To prevent errors from data URLs being too long and to manage API costs, the following steps are taken:
    -   **Page Cap**: The system processes a maximum of the first 5 pages of any uploaded PDF.
    -   **Image Downscaling**: Each page is downscaled to have a maximum dimension (width or height) of 1600 pixels, preserving its aspect ratio.
    -   **Dynamic JPEG Quality**: Images are first encoded with JPEG quality 80. If the resulting file size exceeds a 2MB threshold, the system re-encodes the image at quality 70. If it is still too large, the page is skipped.

-   **JSON-Only Behavior**: The request to the OpenAI model includes a system prompt instructing it to return *only* valid JSON. The service then trims the response to the first valid JSON object (`{...}`) or array (`[...]`) and will throw an error if no valid JSON is found, ensuring the parsing logic only deals with structured data.
