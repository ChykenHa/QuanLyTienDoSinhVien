# Quản Lý Tiến Độ Sinh Viên (Student Progress Management)

A web application for managing student academic progress, built with ASP.NET Core and Entity Framework Core.

## Features

- **Role-based Authentication**: Secure login for Admins, Teachers/Lecturers, and Students.
- **Dashboards**: Tailored views for each user role.
- **Academic Tracking**: Manage enrollments, study plans, and study (semesters, subjects).
- **Notifications**: System-wide notifications for users.
- **Violations**: Record and track student violations.

## Technology Stack

- **Backend**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: SQL Server (LocalDB) via Entity Framework Core
- **Frontend**: Razor Views, Bootstrap

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server (LocalDB supported)

### Installation

1.  Clone the repository:
    ```bash
    git clone https://github.com/ChykenHa/QuanLyTienDoSinhVien.git
    cd QuanLyTienDoSinhVien
    ```

2.  Configure Database:
    Ensure `appsettings.Development.json` points to your local database instance:
    ```json
    "ConnectionStrings": {
      "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=QLTienDoSV;Trusted_Connection=True;"
    }
    ```

3.  Run the application:
    ```bash
    dotnet run --project QuanLyTienDoSinhVien
    ```

4.  Access the app at `http://localhost:5000` (or the port shown in your terminal).

## Default Accounts

- **Admin**: `admin2` / `123456`
- **Student**: (Use registered student accounts)

## License

[MIT](LICENSE)
