# Cruise Experience Survey

A Blazor Server web application for collecting and viewing cruise ship vacation feedback. Surveys are stored in a MSSQL database running in a Docker container that starts automatically with the app.

## Prerequisites

- .NET 10 SDK
- Docker (running)

## Quick Start

```bash
cd ~/Git/DadPOC/CruiseSurvey
dotnet run
```

On first run, the app will:
1. Pull the `mcr.microsoft.com/mssql/server:2022-latest` Docker image (if not already present)
2. Create and start a container named `cruise-survey-mssql`
3. Wait for SQL Server to be ready
4. Create the `CruiseSurveyDb` database and schema automatically

The app is available at **https://localhost:5001** or **http://localhost:5000**.

## Database Connection Details

| Setting            | Value                        |
|--------------------|------------------------------|
| **Server**         | `localhost,1433`             |
| **Database**       | `CruiseSurveyDb`            |
| **User**           | `sa`                         |
| **Password**       | `CruiseSurvey2024!`         |
| **Container Name** | `cruise-survey-mssql`        |
| **Docker Image**   | `mcr.microsoft.com/mssql/server:2022-latest` |

### Connecting with sqlcmd from inside the container

```bash
docker exec -it cruise-survey-mssql /opt/mssql-tools2/bin/sqlcmd \
  -S localhost -U sa -P 'CruiseSurvey2024!' -d CruiseSurveyDb -C
```

### Connecting with Azure Data Studio or SSMS

- **Server:** `localhost,1433`
- **Authentication:** SQL Server Authentication
- **Login:** `sa`
- **Password:** `CruiseSurvey2024!`
- **Trust Server Certificate:** Yes

### Connection string

```
Server=localhost,1433;Database=CruiseSurveyDb;User Id=sa;Password=CruiseSurvey2024!;TrustServerCertificate=True;
```

## Database Schema

### SurveySubmissions

| Column          | Type           | Description                    |
|-----------------|----------------|--------------------------------|
| Id              | int (PK)       | Auto-increment ID              |
| FirstName       | nvarchar(50)   | Guest first name               |
| LastName        | nvarchar(50)   | Guest last name                |
| Email           | nvarchar(200)  | Guest email                    |
| AgeRange        | nvarchar(20)   | Age bracket (e.g. "26-35")     |
| CruiseShipName  | nvarchar(100)  | Ship name                      |
| DepartureDate   | datetime2      | Cruise departure date          |
| NumberOfNights  | int            | Duration of cruise             |
| CompletedAt     | datetime2      | When the survey was completed  |
| AverageRating   | decimal(3,1)   | Computed average across 10 Qs  |

### SurveyAnswers

| Column              | Type           | Description                       |
|---------------------|----------------|-----------------------------------|
| Id                  | int (PK)       | Auto-increment ID                 |
| SurveySubmissionId  | int (FK)       | References SurveySubmissions.Id   |
| QuestionId          | int            | Question number (1-10)            |
| Category            | nvarchar(100)  | Question category name            |
| QuestionText        | nvarchar(500)  | Full question text                |
| Rating              | int            | Star rating (1-5)                 |
| Comment             | nvarchar(2000) | Optional guest comment            |

## Managing the Docker Container

```bash
# Check container status
docker ps -a --filter name=cruise-survey-mssql

# Stop the container
docker stop cruise-survey-mssql

# Start the container
docker start cruise-survey-mssql

# Remove the container (deletes all data)
docker rm -f cruise-survey-mssql

# View container logs
docker logs cruise-survey-mssql
```

## Project Structure

```
CruiseSurvey/
├── Components/
│   ├── App.razor                    # Root component
│   ├── Routes.razor                 # Router
│   ├── _Imports.razor               # Global usings
│   ├── Layout/
│   │   └── MainLayout.razor         # Page layout with nav
│   └── Pages/
│       ├── Index.razor              # Survey wizard (3 steps)
│       └── Results.razor            # Survey results dashboard
├── Data/
│   └── CruiseSurveyDbContext.cs     # EF Core DbContext
├── Models/
│   └── SurveyModel.cs              # Entities and form models
├── Services/
│   ├── DockerSqlServerService.cs    # Docker container management
│   └── SurveyService.cs            # Survey CRUD operations
├── wwwroot/css/site.css             # Styles
├── Program.cs                       # App startup
└── readme.md
```
