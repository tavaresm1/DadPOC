# Newproject — Customer Orders CRUD

A Blazor Web App (.NET 10.0) that provides a master-detail CRUD interface for managing **Customers** and their **Orders**. The application uses Entity Framework Core with SQL Server running in a Docker container that is automatically provisioned at startup.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Local Development](#local-development)
- [Database](#database)
- [Configuration](#configuration)
- [Deployment Options](#deployment-options)
  - [Option 1: Deploy as a Standalone Service](#option-1-deploy-as-a-standalone-service)
  - [Option 2: Deploy with Docker Compose](#option-2-deploy-with-docker-compose)
  - [Option 3: Deploy to Azure App Service](#option-3-deploy-to-azure-app-service)
  - [Option 4: Deploy to a Linux Server with systemd](#option-4-deploy-to-a-linux-server-with-systemd)
- [Environment Variables](#environment-variables)
- [Troubleshooting](#troubleshooting)

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (preview)
- [Docker](https://docs.docker.com/get-docker/) (for local development — the app auto-creates a SQL Server container)
- A SQL Server instance (for production deployments where you supply your own connection string)

## Project Structure

```
Newproject/
├── Components/
│   ├── Layout/
│   │   └── MainLayout.razor          # App shell with Bootstrap navbar
│   ├── Pages/
│   │   ├── CustomerList.razor         # Home page — list, add, edit, delete customers
│   │   └── CustomerDetail.razor       # Customer detail — order CRUD
│   ├── App.razor                      # Root HTML document
│   ├── Routes.razor                   # Blazor router
│   └── _Imports.razor                 # Global using directives
├── Data/
│   └── AppDbContext.cs                # EF Core DbContext (Customers + Orders)
├── Models/
│   ├── Customer.cs                    # Master entity
│   └── Order.cs                       # Detail entity
├── Properties/
│   └── launchSettings.json            # Dev server URLs (https://localhost:5003)
├── Services/
│   └── DockerSqlServerService.cs      # Auto-provisions SQL Server in Docker
├── wwwroot/
│   └── css/site.css                   # Custom styles
├── appsettings.json                   # Logging configuration
├── Newproject.csproj                  # Project file (net10.0, EF Core SQL Server)
├── Program.cs                         # App entry point and service configuration
└── README.md
```

## Local Development

1. **Clone the repository** and navigate to the project:

   ```bash
   cd /path/to/DadPOC/Newproject
   ```

2. **Run the application:**

   ```bash
   dotnet run
   ```

   On first run, the application will:
   - Pull the `mcr.microsoft.com/mssql/server:2022-latest` Docker image (if not already present)
   - Create a Docker container named `newproject-mssql` on port **1434**
   - Wait for SQL Server to become ready
   - Create the `NewprojectDb` database and schema automatically via `EnsureCreatedAsync()`

3. **Open in your browser:**

   ```
   https://localhost:5003
   ```

4. **Use the app:**
   - The home page lists all customers with inline add/edit/delete
   - Click **Details** on a customer to view and manage their orders
   - Orders support full CRUD with status tracking (Pending, Processing, Shipped, Delivered, Cancelled)

## Database

### Schema

**Customer** (master):
| Column    | Type           | Constraints            |
|-----------|----------------|------------------------|
| Id        | int            | Primary key, auto-inc  |
| FirstName | nvarchar(100)  | Required               |
| LastName  | nvarchar(100)  | Required               |
| Email     | nvarchar(200)  | Required, unique index |
| Phone     | nvarchar(20)   | Nullable               |
| CreatedAt | datetime2      | Default: UTC now       |

**Order** (detail):
| Column      | Type           | Constraints               |
|-------------|----------------|---------------------------|
| Id          | int            | Primary key, auto-inc     |
| Description | nvarchar(200)  | Required                  |
| Amount      | decimal(18,2)  | Required, min 0.01        |
| OrderDate   | datetime2      | Default: UTC now          |
| Status      | nvarchar(50)   | Required, default Pending |
| CustomerId  | int            | Foreign key to Customer   |

Deleting a customer cascades to delete all associated orders.

### Local Docker Container

The development SQL Server container uses:
- **Container name:** `newproject-mssql`
- **Host port:** `1434` (avoids conflict with CruiseSurvey on 1433)
- **SA password:** `NewProject2024!`
- **Database:** `NewprojectDb`

Connection string (auto-configured in development):
```
Server=localhost,1434;Database=NewprojectDb;User Id=sa;Password=NewProject2024!;TrustServerCertificate=True;
```

### Connecting with a SQL Client

You can connect to the development database using any SQL client (Azure Data Studio, SSMS, DBeaver) with:
- **Server:** `localhost,1434`
- **Authentication:** SQL Login
- **User:** `sa`
- **Password:** `NewProject2024!`

## Configuration

### appsettings.json

The default configuration handles logging levels. The database connection is managed by `DockerSqlServerService` for local development.

### launchSettings.json

Development URLs are configured as:
- HTTPS: `https://localhost:5003`
- HTTP: `http://localhost:5002`

## Deployment Options

### Option 1: Deploy as a Standalone Service

For environments where you have a dedicated SQL Server instance.

1. **Publish the application:**

   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Set the connection string** via environment variable:

   ```bash
   export ConnectionStrings__DefaultConnection="Server=your-sql-server;Database=NewprojectDb;User Id=your-user;Password=your-password;TrustServerCertificate=True;"
   ```

3. **Modify `Program.cs`** for production to read the connection string from configuration instead of `DockerSqlServerService`:

   ```csharp
   // Replace the DbContextFactory registration with:
   builder.Services.AddDbContextFactory<AppDbContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

   // Remove or conditionally skip the DockerSqlServerService.EnsureRunningAsync() call
   ```

4. **Run the published app:**

   ```bash
   cd ./publish
   dotnet Newproject.dll --urls "http://0.0.0.0:5000"
   ```

### Option 2: Deploy with Docker Compose

Create a `docker-compose.yml` in the project root:

```yaml
services:
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "NewProject2024!"
    ports:
      - "1434:1433"
    volumes:
      - sqldata:/var/opt/mssql

  web:
    build: .
    ports:
      - "8080:8080"
    environment:
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Server=db;Database=NewprojectDb;User Id=sa;Password=NewProject2024!;TrustServerCertificate=True;
    depends_on:
      - db

volumes:
  sqldata:
```

Create a `Dockerfile` in the project root:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Newproject.dll"]
```

Then modify `Program.cs` to read from configuration (as described in Option 1) and run:

```bash
docker compose up -d
```

The app will be available at `http://localhost:8080`.

### Option 3: Deploy to Azure App Service

1. **Create Azure resources:**

   ```bash
   az group create --name newproject-rg --location eastus

   az sql server create \
     --name newproject-sql \
     --resource-group newproject-rg \
     --admin-user sqladmin \
     --admin-password "YourStrongPassword123!"

   az sql db create \
     --resource-group newproject-rg \
     --server newproject-sql \
     --name NewprojectDb \
     --service-objective S0

   az sql server firewall-rule create \
     --resource-group newproject-rg \
     --server newproject-sql \
     --name AllowAzureServices \
     --start-ip-address 0.0.0.0 \
     --end-ip-address 0.0.0.0

   az appservice plan create \
     --name newproject-plan \
     --resource-group newproject-rg \
     --sku B1 \
     --is-linux

   az webapp create \
     --resource-group newproject-rg \
     --plan newproject-plan \
     --name newproject-app \
     --runtime "DOTNETCORE:10.0"
   ```

2. **Configure the connection string:**

   ```bash
   az webapp config connection-string set \
     --resource-group newproject-rg \
     --name newproject-app \
     --settings DefaultConnection="Server=tcp:newproject-sql.database.windows.net,1433;Database=NewprojectDb;User Id=sqladmin;Password=YourStrongPassword123!;Encrypt=True;TrustServerCertificate=False;" \
     --connection-string-type SQLAzure
   ```

3. **Publish and deploy:**

   ```bash
   dotnet publish -c Release -o ./publish
   cd ./publish
   zip -r ../deploy.zip .
   az webapp deploy \
     --resource-group newproject-rg \
     --name newproject-app \
     --src-path ../deploy.zip \
     --type zip
   ```

### Option 4: Deploy to a Linux Server with systemd

1. **Publish:**

   ```bash
   dotnet publish -c Release -o /var/www/newproject
   ```

2. **Create a systemd service file** at `/etc/systemd/system/newproject.service`:

   ```ini
   [Unit]
   Description=Newproject Customer Orders App
   After=network.target

   [Service]
   WorkingDirectory=/var/www/newproject
   ExecStart=/usr/bin/dotnet /var/www/newproject/Newproject.dll
   Restart=always
   RestartSec=10
   SyslogIdentifier=newproject
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production
   Environment=ASPNETCORE_URLS=http://localhost:5000
   Environment=ConnectionStrings__DefaultConnection=Server=your-sql-server;Database=NewprojectDb;User Id=your-user;Password=your-password;TrustServerCertificate=True;

   [Install]
   WantedBy=multi-user.target
   ```

3. **Enable and start the service:**

   ```bash
   sudo systemctl enable newproject
   sudo systemctl start newproject
   ```

4. **Set up a reverse proxy** (Nginx example):

   ```nginx
   server {
       listen 80;
       server_name your-domain.com;

       location / {
           proxy_pass http://localhost:5000;
           proxy_http_version 1.1;
           proxy_set_header Upgrade $http_upgrade;
           proxy_set_header Connection "upgrade";
           proxy_set_header Host $host;
           proxy_set_header X-Real-IP $remote_addr;
           proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
           proxy_set_header X-Forwarded-Proto $scheme;
       }
   }
   ```

   The `Upgrade` and `Connection` headers are required for Blazor Server's SignalR WebSocket connection.

## Environment Variables

| Variable | Description | Default (dev) |
|----------|-------------|---------------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Development` |
| `ASPNETCORE_URLS` | Listen URLs | `https://localhost:5003;http://localhost:5002` |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | Auto-configured by DockerSqlServerService |

## Troubleshooting

### Docker container won't start

```bash
# Check if the container already exists
docker ps -a | grep newproject-mssql

# View container logs
docker logs newproject-mssql

# Remove and let the app recreate it
docker rm -f newproject-mssql
```

### Port 1434 is already in use

Another service is using port 1434. Either stop that service or change the `HostPort` constant in `Services/DockerSqlServerService.cs`.

### Database connection timeout

SQL Server takes 10-30 seconds to start inside Docker on first run. The app retries up to 30 times (2 seconds apart). If it still times out:

```bash
# Verify the container is running
docker ps | grep newproject-mssql

# Test connectivity manually
docker exec newproject-mssql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "NewProject2024!" -Q "SELECT 1" -C
```

### Build errors about .NET 10.0

Ensure you have the .NET 10.0 preview SDK installed:

```bash
dotnet --version  # Should show 10.0.x
```

If not, download it from https://dotnet.microsoft.com/download/dotnet/10.0.

### Blazor SignalR disconnects in production

Ensure your reverse proxy is configured to support WebSockets (see the Nginx example in Option 4). Without WebSocket support, Blazor Server will fall back to long polling, which degrades performance.
