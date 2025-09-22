# CMetalsWS - Metal Logistics and Production Management System

CMetalsWS is a comprehensive web application built with Blazor for managing logistics, production scheduling, and warehouse operations, tailored for the metal processing industry.

## System Overview

The application provides a full suite of tools to manage the entire operational workflow, from initial system setup to final delivery. It is designed with a role-based access model, ensuring that users only see the information and tools relevant to their tasks.

The core process flow is as follows:
1.  **Configuration**: Administrators set up foundational data, including branches, shifts, machines, trucks, users, roles, and logistics routes (customers, destinations).
2.  **Order Ingestion**: Work Orders and Picking Lists are entered into the system. The system features a powerful PDF parsing tool to automatically ingest picking lists from external documents.
3.  **Task Management**: The system generates tasks (e.g., Pulling Tasks) from the orders and places them in a queue for assignment.
4.  **Real-time Scheduling**: Managers and operators use live, real-time schedules for different production lines (CTL, Slitter) and warehouse operations (Pulling, Loading).
5.  **Load Planning**: Planners group picked items into loads based on destination, assign them to trucks, and schedule them for delivery.
6.  **Monitoring**: A central dashboard provides an overview of operations, and a full audit trail of tasks is maintained.

## Features

### 1. Authentication and User Management
- **User Login/Logout**: Secure email/password authentication.
- **Role-Based Access Control (RBAC)**: Fine-grained permissions system to control access to every feature.
- **User Administration**: Admins can create, edit, and manage users.
- **Role Administration**: Admins can define roles (e.g., 'Administrator', 'Planner', 'Operator') and assign specific permissions to them.
- **User Profile**: Users can manage their own profile information.

### 2. Core Data Configuration (System Panel)
- **Branch Management**: Manage multiple company locations.
- **Shift Management**: Define work shifts.
- **Machine Management**: Manage production machines and their schedules.
- **Truck Management**: Manage the fleet of delivery trucks.
- **Customer Management**: Maintain a customer database and import new customers.
- **Logistics Configuration**: Define Destination Groups and Regions to streamline delivery planning.
- **Product & Inventory Configuration**: Define item relationships (bills of materials) and view inventory.

### 3. Operations Management
- **Work Order Management**: Track production work orders.
- **Picking List Management**:
    - Manage picking lists for customer orders.
    - **PDF Upload & Parsing**: Upload PDF picking lists and have the system automatically parse and import the data.
- **Task Management**:
    - **Pulling Tasks**: Manage tasks for pulling items from inventory.
    - **Sheet Pulling Queue**: A dedicated queue for managing and assigning sheet metal pulling tasks.

### 4. Real-time Scheduling
- **Live-Updating Schedules**: All schedules are updated in real-time using SignalR.
- **CTL Schedule**: A schedule view for the Cut-to-Length machine.
- **Slitter Schedule**: A schedule for the Slitter machine.
- **Pulling Schedule**: A schedule for material pulling tasks.
- **Loads Schedule**: A schedule for planned deliveries.

### 5. Logistics and Planning
- **Dynamic Load Planning**: A powerful interface for planners to build delivery loads based on destination regions.

### 6. Monitoring and Communication
- **Dashboard**: A central page for a high-level overview of operations.
- **Notifications & Chat (Partially Implemented)**: The application includes infrastructure for user notifications and real-time chat, which can be enabled to improve team communication.

## Suggested Improvements

Based on a review of the system's architecture and features, here are several suggestions for future enhancements:

1.  **Activate and Integrate Communication Features**: The `ChatHub` and `NotificationBell` are already partially implemented. Completing these features would enable real-time communication between planners, managers, and operators, reducing delays and improving coordination.
2.  **Enhance the Dashboard**: The dashboard could be evolved from a static overview into a dynamic, role-based hub displaying KPIs like machine uptime, operator efficiency, and order fulfillment rates.
3.  **Introduce Automated Task Assignment**: The current workflow relies on manual task assignment. An intelligent assignment engine could be built to automatically assign tasks based on operator availability, machine load, or skill set, optimizing resource allocation.
4.  **Expand Inventory Management**: The inventory module could be enhanced with features like bin/location tracking, automatic stock decrementation upon task completion, and low-stock level alerts to prevent shortages.
5.  **Develop a Dedicated Reporting Module**: A dedicated module for generating, exporting, and scheduling historical reports (e.g., production output, delivery performance) would provide valuable insights for business analysis.
6.  **Provide an External API**: Exposing a secure REST or GraphQL API would allow for integration with other business systems, such as an external ERP or an accounting platform.

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server (or SQL Server Express)
- A tool to run `.sql` scripts or manage the database (e.g., SQL Server Management Studio, Azure Data Studio).

### Setup
1.  **Clone the repository:**
    ```bash
    git clone <repository-url>
    cd CMetalsWS
    ```
2.  **Configure the database connection:**
    -   Find the `appsettings.json` file.
    -   Locate the `ConnectionStrings` section.
    -   Update the `DefaultConnection` string to point to your SQL Server instance.
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CMetalsWS;Trusted_Connection=True;MultipleActiveResultSets=true"
    }
    ```
3.  **Apply database migrations:**
    -   The application uses Entity Framework Core. Migrations are applied automatically on startup. Alternatively, you can run them from the command line:
    ```bash
    dotnet ef database update
    ```
4.  **Run the application:**
    ```bash
    dotnet run
    ```
5.  **Access the application:**
    -   Open a web browser and navigate to the URL provided in the console output (e.g., `https://localhost:5001`).

### Default Admin User
On first run, the system will seed the database with default data, including:
-   An administrator user.
-   Default roles and permissions.

You will need to consult the `IdentityDataSeeder.cs` file to find the credentials for the default admin user to log in for the first time.
