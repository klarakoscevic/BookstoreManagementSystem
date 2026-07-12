# Bookstore Management System

A comprehensive .NET Web API for managing bookstore operations including books, authors, genres, reviews, and automated book imports.

## Table of Contents
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Setup Instructions](#setup-instructions)
- [Running the Application](#running-the-application)
- [Testing](#testing)
- [API Documentation](#api-documentation)
- [Authentication & Authorization](#authentication--authorization)
- [Scheduled Tasks](#scheduled-tasks)
- [Project Structure](#project-structure)

## Features

### Core Functionality
- **Book Management**: CRUD operations with price updates, author associations, and genre classifications
- **Author Management**: Track authors with birth years and book associations
- **Genre Management**: Organize books by multiple genres
- **Review System**: Rating system (1-5) with text descriptions for books
- **Top 10 Books**: Get the highest-rated books using raw SQL queries
- **Scheduled Import**: Automated hourly book imports from external sources

### Technical Features
- JWT-based authentication and role-based authorization
- Structured logging with Serilog (console + file output)
- Swagger/OpenAPI documentation with JWT support
- Dependency injection architecture
- Entity Framework Core with SQL Server 2025
- Soft delete implementation (IsActive flags)
- Comprehensive unit and integration tests
- Automated database migrations

## Technology Stack

- **.NET 10.0** - Target framework
- **ASP.NET Core Web API** - REST API framework
- **Entity Framework Core 10.0** - ORM for database operations
- **SQL Server 2025 Express Edition (RTM-CU3)** - Relational database
- **Serilog 10.0** - Structured logging framework
- **Quartz.NET 3.18.2** - Scheduled task management
- **JWT Bearer Authentication** - Security and authorization
- **Swagger/Swashbuckle 7.2** - API documentation
- **xUnit** - Unit testing framework
- **FluentAssertions** - Fluent test assertions
- **Moq** - Mocking framework for unit tests

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- [Visual Studio 2026](https://visualstudio.microsoft.com/) Community (version 18.7.3 or later)
  - Workload: **ASP.NET and web development**
  - Workload: **.NET desktop development**
- **Microsoft SQL Server 2025 Express Edition** (RTM-CU3) or any SQL Server 2019+ edition
  - With SQL Server Management Studio (SSMS) recommended
- [Git](https://git-scm.com/) (for cloning the repository)

## Setup Instructions

### 1. Clone the Repository

```powershell
git clone https://github.com/klarakoscevic/BookstoreManagementSystem.git
cd BookstoreManagementSystem
```

### 2. Configure the Database Connection

Update the connection string in `appsettings.json` to match your SQL Server 2025 instance.

For **SQL Server 2025 Express Edition** with Windows Authentication:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=BookstoreDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

For **SQL Server 2025** with SQL Authentication:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BookstoreDb;User Id=your_username;Password=your_password;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

For **LocalDB** (if you prefer):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BookstoreDb2;Trust

ed_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3. Verify SQL Server is Running

```powershell
# Check SQL Server service status
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Test connection
sqlcmd -S localhost\SQLEXPRESS -Q "SELECT @@VERSION"
```

### 4. Restore NuGet Packages

```powershell
dotnet restore
```

### 5. Apply Database Migrations

```powershell
cd BookstoreManagementSystem
dotnet ef database update
```

This will create the database, apply migrations, and seed initial data.

### 6. Build the Solution

```powershell
dotnet build
```

## Running the Application

### Using Visual Studio 2026

1. Open `BookstoreManagementSystem.slnx`
2. Set `BookstoreManagementSystem` as the startup project
3. Press `F5`
4. Swagger UI opens automatically at `https://localhost:7xxx`

### Using Command Line

```powershell
cd BookstoreManagementSystem
dotnet run
```

The API will be available at `https://localhost:7xxx` (check console for exact port).

## Testing

### Run All Tests

```powershell
dotnet test
```

### Run Tests with Detailed Output

```powershell
dotnet test --verbosity detailed
```

### Test Coverage

- **30+ Unit Tests**: Service layer logic
- **7 Integration Test Suites**: End-to-end API tests

## API Documentation

Access Swagger UI at: `https://localhost:7xxx`

### Main Endpoints

- **POST /api/auth/register** - Register new user
- **POST /api/auth/login** - Login and get JWT token
- **GET /api/books** - Get all books (with authors, genres, ratings)
- **GET /api/books/top10** - Top 10 books by rating (raw SQL)
- **POST /api/books** - Create book (ReadWrite role required)
- **PUT /api/books/{id}/price** - Update book price
- **GET /api/authors** - Get all authors
- **GET /api/genres** - Get all genres
- **GET /api/reviews** - Get all reviews

## Authentication & Authorization

### Roles

- **Read**: Access to all GET endpoints
- **ReadWrite**: Full CRUD access

### Usage

1. Register: `POST /api/auth/register`
2. Login: `POST /api/auth/login` to get JWT token
3. Use token in Authorization header: `Bearer {token}`

## Scheduled Tasks

### Book Import Job

- **Schedule**: Every hour (`0 0 * * * ?`)
- **Features**:
  - Simulates fetching from external API
  - Title matching (case-insensitive, trimmed)
  - Skips duplicates
  - Creates new authors/genres as needed

### Manual Trigger

```http
POST /api/import/trigger
Authorization: Bearer {token}
```

## Project Structure

```
BookstoreManagementSystem/
+¦¦ BookstoreManagementSystem/        # Main API
-   +¦¦ Controllers/                  # API endpoints
-   +¦¦ Data/                        # DbContext
-   +¦¦ DTOs/                        # Data Transfer Objects
-   +¦¦ Jobs/                        # Quartz.NET jobs
-   +¦¦ Models/                      # Domain entities
-   +¦¦ Repositories/                # Data access
-   +¦¦ Services/                    # Business logic
-   L¦¦ Program.cs                   # Entry point
-
+¦¦ BookstoreManagementSystem.Tests/ # Tests
-   +¦¦ IntegrationTests/           # API tests
-   L¦¦ UnitTests/                  # Service tests
-
L¦¦ README.md                        # This file
```

## Troubleshooting

### Database Connection Issues

```powershell
# Check SQL Server is running
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Drop and recreate database
dotnet ef database drop --force
dotnet ef database update
```

### Authentication Issues

- Ensure token is in Authorization header
- Check token hasn't expired (24 hours)
- Use "Read Write" role for POST/PUT/DELETE operations

## Assignment Requirements Checklist

- .NET Web API (.NET 10.0)
- SQL Server 2025 Express Edition
- Books, Authors, Genres, Reviews with proper relationships
- CRUD endpoints with GET returning: Title, Authors, Genres, Average Rating
- Update endpoint for book price only
- Top 10 books using raw SQL
- JWT authentication with Read/ReadWrite roles
- Structured logging (Serilog)
- Swagger documentation
- Dependency injection
- Scheduled import (Quartz.NET, every hour)
- Title matching with typo handling
- 30+ unit tests
- 7 integration test suites
- README with setup and test instructions

## Author

**Klara Koscevic**
- GitHub: [@klarakoscevic](https://github.com/klarakoscevic)

