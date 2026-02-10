# Demand Management Application

A full-stack demand management system built with .NET 10.0, Blazor Server, and ASP.NET Core REST API. The application enables organizations to submit, assess, prioritize, and approve IT demand requests with role-based access control, financial analysis (NPV), resource/capacity planning, and full audit trails.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [Authentication & Authorization](#authentication--authorization)
- [Domain Model](#domain-model)
- [API Reference](#api-reference)
- [UI Pages](#ui-pages)
- [Features](#features)
- [Configuration](#configuration)
- [Testing](#testing)

---

## Architecture Overview

The application follows a clean architecture pattern with four main layers:

```
DemandManagement2.Ui (Blazor Server - Port 5235)
        |
        | HTTP/REST + JWT Bearer
        v
DemandManagement2.Api (ASP.NET Core Web API - Port 5182)
        |
        | Entity Framework Core
        v
DemandManagement2.Infrastructure (Data Access)
        |
        v
DemandManagement2.Domain (Entities & Enums)
```

- **UI** communicates with the **API** over HTTP using JWT Bearer tokens.
- **API** uses **Infrastructure** (EF Core + SQL Server) for data persistence.
- **Domain** contains all entity models and enums shared across layers.

---

## Technology Stack

| Component        | Technology                              |
|------------------|-----------------------------------------|
| Framework        | .NET 10.0                               |
| Frontend         | Blazor Server (InteractiveServer)       |
| Backend          | ASP.NET Core Web API                    |
| Database         | SQL Server (LocalDB/SQL Express)        |
| ORM              | Entity Framework Core 10.0              |
| Authentication   | JWT Bearer Tokens                       |
| Password Hashing | BCrypt.Net-Next                         |
| API Docs         | Swagger / Swashbuckle                   |
| Testing          | xUnit                                   |

---

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or SQL Express)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DemandManagementApp2
   ```

2. **Update the connection string** (if needed)

   Edit `DemandManagement2.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\sqlexpress;Database=DemandManagementDatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Apply database migrations**
   ```bash
   cd DemandManagement2.Api
   dotnet ef database update
   ```

4. **Run the API**
   ```bash
   cd DemandManagement2.Api
   dotnet run
   ```
   The API starts on `http://localhost:5182`. Swagger UI is available at `http://localhost:5182/swagger`.

5. **Run the UI** (in a separate terminal)
   ```bash
   cd DemandManagement2.Ui
   dotnet run
   ```
   The UI starts on `http://localhost:5235`.

6. **Register a user** via the Register page and start using the application.

---

## Project Structure

```
DemandManagementApp2/
|-- DemandManagement2.Domain/          # Entities & enums
|   |-- Entities/
|       |-- DemandRequest.cs           # Primary aggregate root
|       |-- Assessment.cs              # Scoring & NPV analysis
|       |-- ApprovalDecision.cs        # Approval workflow
|       |-- DecisionNote.cs            # Meeting notes
|       |-- DemandEvent.cs             # Activity timeline / audit trail
|       |-- DemandAttachment.cs        # File attachments
|       |-- Resource.cs                # Resource pool
|       |-- ResourceAllocation.cs      # Capacity allocations
|       |-- User.cs                    # User accounts & roles
|
|-- DemandManagement2.Infrastructure/  # Data access layer
|   |-- Data/
|       |-- AppDbContext.cs            # EF Core DbContext
|       |-- Migrations/               # Database migrations
|
|-- DemandManagement2.Api/             # REST API
|   |-- Controllers/
|   |   |-- AuthController.cs          # Registration & login
|   |   |-- DemandRequestsController.cs# CRUD for demands
|   |   |-- AssessmentsController.cs   # Scoring & NPV
|   |   |-- ApprovalsController.cs     # Approval decisions
|   |   |-- DecisionNotesController.cs # Meeting notes
|   |   |-- AttachmentsController.cs   # File upload/download
|   |   |-- PrioritizationController.cs# Priority-sorted view
|   |   |-- DashboardController.cs     # KPIs & aging
|   |   |-- ResourcesController.cs     # Resource management
|   |   |-- CapacityController.cs      # Capacity planning
|   |-- Dtos/                          # Data transfer objects
|   |-- Services/
|       |-- ScoringService.cs          # Weighted scoring algorithm
|
|-- DemandManagement2.Ui/              # Blazor Server frontend
|   |-- Components/
|   |   |-- Pages/
|   |   |   |-- Home.razor             # Landing page
|   |   |   |-- Login.razor            # Login page
|   |   |   |-- Register.razor         # Registration page
|   |   |   |-- Dashboard.razor        # KPI dashboard
|   |   |   |-- Demands.razor          # Demand list
|   |   |   |-- DemandDetails.razor    # Demand detail + assessment + approval
|   |   |   |-- NewDemand.razor        # Create demand form
|   |   |   |-- Prioritization.razor   # Priority board
|   |   |   |-- Capacity.razor         # Capacity planning
|   |   |-- Layout/
|   |-- Services/
|       |-- DemandApiClient.cs         # HTTP client for API
|       |-- AuthService.cs             # Login/register/logout
|       |-- JwtAuthenticationStateProvider.cs # Blazor auth state
|       |-- TokenStorageService.cs     # Secure token storage
|
|-- DemandManagement2.Tests/           # Unit tests (xUnit)
    |-- ScoringServiceTests.cs         # Weighted score tests
    |-- NpvCalculationTests.cs         # NPV calculation tests
```

---

## Authentication & Authorization

### Roles

| Role        | Description                                                |
|-------------|------------------------------------------------------------|
| **Admin**     | Full access: manage all demands, assess, approve, delete, manage resources & capacity |
| **Assessor**  | Assess demands, approve/reject, request info, manage notes, manage resources & capacity |
| **Requester** | Submit demands, edit own demands (Intake/NeedsInfo only), view own demands only |

### Role-Based Access Matrix

| Feature                    | Admin | Assessor | Requester |
|----------------------------|-------|----------|-----------|
| Dashboard                  | Full  | Full     | Own demands only |
| View all demands           | Yes   | Yes      | No (own only) |
| View demand details        | Any   | Any      | Own only |
| Submit new demand          | Yes   | Yes      | Yes |
| Edit demand                | Yes   | Yes      | Own (Intake/NeedsInfo) |
| Assess demand              | Yes   | Yes      | No |
| Approve/Reject demand      | Yes   | Yes      | No |
| Request additional info    | Yes   | Yes      | No |
| Delete demand              | Yes   | Yes      | No |
| Prioritization page        | Yes   | Yes      | Access denied message |
| Capacity Planning page     | Yes   | Yes      | Access denied message |
| Manage resources           | Yes   | Yes      | No |
| Download attachments       | Yes   | Yes      | Yes (own demands) |
| Delete attachments         | Yes   | Yes      | No |
| Decision notes             | Yes   | Yes      | View only |

### Auth Flow

1. User registers at `/register` with name, email, password, and role.
2. User logs in at `/login` - API returns a JWT token (8-hour expiry).
3. Token is stored in `ProtectedSessionStorage` (server-side encrypted).
4. `JwtAuthenticationStateProvider` decodes the JWT and exposes user claims.
5. `DemandApiClient` attaches the Bearer token to every API request.
6. API endpoints enforce role restrictions via `[Authorize(Roles = "...")]`.
7. API enforces demand ownership - Requesters can only access their own demands (server-side filtering).
8. UI components conditionally render based on `AuthenticationState`.
9. Navigation menu hides restricted pages (Prioritization, Capacity) from Requesters.
10. Restricted pages show meaningful "Access Restricted" messages with redirect links.

### JWT Configuration

| Setting           | Value                       |
|-------------------|-----------------------------|
| Issuer            | `DemandManagement2.Api`     |
| Audience          | `DemandManagement2.Ui`      |
| Expiration        | 480 minutes (8 hours)       |
| Signing Algorithm | HMAC-SHA256                 |

---

## Domain Model

### Entities & Relationships

```
User (Guid Id, FullName, Email, PasswordHash, Role)

DemandRequest (Guid Id, Title, ProblemStatement, Type, Status, ...)
  |-- Assessment (1:1) - Scoring + NPV financial analysis
  |-- ApprovalDecision (1:1) - Approve/Reject/OnHold
  |-- DecisionNote (1:N) - Meeting notes & decisions
  |-- DemandEvent (1:N) - Activity timeline / audit trail
  |-- DemandAttachment (1:N) - File attachments
  |-- ResourceAllocation (1:N) - Resource assignments

Resource (Guid Id, Name, Role, Department, CapacityHoursPerMonth)
  |-- ResourceAllocation (1:N)
```

### Demand Statuses

| Status         | Description                                  |
|----------------|----------------------------------------------|
| `Intake`       | Newly submitted, awaiting review             |
| `UnderReview`  | Assessment in progress                       |
| `Prioritized`  | Scored and ranked                             |
| `Approved`     | Approved for implementation                  |
| `Backlog`      | On hold / deferred                           |
| `Rejected`     | Not approved                                 |
| `NeedsInfo`    | Additional information requested from submitter |

### Demand Types

| Type              | Description                        |
|-------------------|------------------------------------|
| `Project`         | New project initiative             |
| `Enhancement`     | Improvement to existing system     |
| `Service`         | Service request                    |
| `ResourceRequest` | Request for additional resources   |

### Scoring Algorithm

Demands are scored on a 0-100 scale using weighted criteria:

| Criterion            | Weight | Direction           |
|----------------------|--------|---------------------|
| Business Value       | 30%    | Higher is better    |
| Strategic Alignment  | 25%    | Higher is better    |
| Urgency              | 15%    | Higher is better    |
| Cost Impact          | 10%    | Lower is better (inverted) |
| Risk                 | 10%    | Lower is better (inverted) |
| Resource Need        | 10%    | Lower is better (inverted) |

### NPV Calculation

Financial viability is assessed using Net Present Value:

```
NPV = -InitialCost + SUM(AnnualBenefit / (1 + DiscountRate)^t) for t = 1..ProjectYears
```

---

## API Reference

Base URL: `http://localhost:5182`

### Auth (`/api/auth`)

| Method | Endpoint              | Auth     | Description              |
|--------|-----------------------|----------|--------------------------|
| POST   | `/api/auth/register`  | Public   | Register a new user      |
| POST   | `/api/auth/login`     | Public   | Login and receive JWT     |

### Demands (`/api/demands`)

| Method | Endpoint                            | Auth             | Description                        |
|--------|-------------------------------------|------------------|------------------------------------|
| GET    | `/api/demands`                      | Authenticated    | List demands (Requesters: own only; filter: ?status, ?type) |
| GET    | `/api/demands/{id}`                 | Authenticated    | Get demand details (Requesters: own only, 403 otherwise) |
| POST   | `/api/demands`                      | Authenticated    | Create a new demand                |
| PUT    | `/api/demands/{id}`                 | Authenticated    | Update demand (Requester: Intake/NeedsInfo only) |
| PATCH  | `/api/demands/{id}/request-info`    | Admin, Assessor  | Request additional info from submitter |
| DELETE | `/api/demands/{id}`                 | Admin, Assessor  | Delete demand and all related data |

### Assessments (`/api/demands/{demandId}/assessment`)

| Method | Endpoint                                     | Auth             | Description                    |
|--------|----------------------------------------------|------------------|--------------------------------|
| GET    | `/api/demands/{demandId}/assessment`         | Admin, Assessor  | Get assessment                 |
| POST   | `/api/demands/{demandId}/assessment`         | Admin, Assessor  | Create/update assessment (auto-calculates NPV & score) |

### Approvals (`/api/demands/{demandId}/approval`)

| Method | Endpoint                                    | Auth             | Description                    |
|--------|---------------------------------------------|------------------|--------------------------------|
| GET    | `/api/demands/{demandId}/approval`          | Admin, Assessor  | Get approval decision          |
| POST   | `/api/demands/{demandId}/approval`          | Admin, Assessor  | Create/update approval (syncs demand status) |

### Decision Notes (`/api/demands/{demandId}/notes`)

| Method | Endpoint                                    | Auth             | Description                    |
|--------|---------------------------------------------|------------------|--------------------------------|
| GET    | `/api/demands/{demandId}/notes`             | Authenticated    | List all notes                 |
| GET    | `/api/demands/{demandId}/notes/{id}`        | Authenticated    | Get single note                |
| POST   | `/api/demands/{demandId}/notes`             | Authenticated    | Create note                    |
| PUT    | `/api/demands/{demandId}/notes/{id}`        | Authenticated    | Update note                    |
| DELETE | `/api/demands/{demandId}/notes/{id}`        | Authenticated    | Delete note                    |

### Attachments (`/api/demands/{demandId}/attachments`)

| Method | Endpoint                                              | Auth             | Description                    |
|--------|-------------------------------------------------------|------------------|--------------------------------|
| POST   | `/api/demands/{demandId}/attachments`                 | Authenticated    | Upload file (max 10MB)         |
| GET    | `/api/demands/{demandId}/attachments/{attachmentId}`  | Public           | Download file (GUID-based URLs) |
| DELETE | `/api/demands/{demandId}/attachments/{attachmentId}`  | Admin, Assessor  | Delete file                    |

**Allowed file types:** `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.png`, `.jpg`, `.jpeg`, `.txt`, `.csv`, `.zip`

### Prioritization (`/api/prioritization`)

| Method | Endpoint              | Auth          | Description                                    |
|--------|-----------------------|---------------|------------------------------------------------|
| GET    | `/api/prioritization` | Authenticated | Demands sorted by WeightedScore, Urgency, Date |

### Dashboard (`/api/dashboard`)

| Method | Endpoint                   | Auth          | Description                      |
|--------|----------------------------|---------------|----------------------------------|
| GET    | `/api/dashboard/summary`   | Authenticated | Total count and status breakdown |
| GET    | `/api/dashboard/aging`     | Authenticated | Demand aging and overdue flags   |

### Resources (`/api/resources`)

| Method | Endpoint               | Auth  | Description                        |
|--------|------------------------|-------|------------------------------------|
| GET    | `/api/resources`       | Admin, Assessor | List all active resources          |
| GET    | `/api/resources/{id}`  | Admin, Assessor | Get single resource                |
| POST   | `/api/resources`       | Admin, Assessor | Create resource                    |
| PUT    | `/api/resources/{id}`  | Admin, Assessor | Update resource                    |
| DELETE | `/api/resources/{id}`  | Admin, Assessor | Soft-delete resource (IsActive=false) |

### Capacity (`/api/capacity`)

| Method | Endpoint                          | Auth          | Description                       |
|--------|-----------------------------------|---------------|-----------------------------------|
| GET    | `/api/capacity/summary`           | Authenticated | Monthly capacity by department    |
| GET    | `/api/capacity/allocations`       | Authenticated | Resource allocations for month    |
| POST   | `/api/capacity/allocations`       | Authenticated | Create allocation                 |
| DELETE | `/api/capacity/allocations/{id}`  | Authenticated | Delete allocation                 |
| GET    | `/api/capacity/forecast`          | Authenticated | 6-month capacity forecast         |

---

## UI Pages

| Route                    | Page              | Access        | Description                                        |
|--------------------------|-------------------|---------------|----------------------------------------------------|
| `/`                      | Home              | Public        | Landing page with overview KPIs and navigation     |
| `/login`                 | Login             | Public        | Email/password login form                          |
| `/register`              | Register          | Public        | User registration with role selection              |
| `/dashboard`             | Dashboard         | Authenticated | KPI cards, highlights, top priorities (role-aware welcome message) |
| `/demands`               | Demands           | Authenticated | Demand list (Requesters see own only, Admin/Assessor see all) |
| `/demands/new`           | New Demand        | Authenticated | Create demand form with file upload (all roles) |
| `/demands/{id}`          | Demand Details    | Authenticated | Full detail view (Requesters: own only, 403 otherwise) |
| `/prioritization`        | Prioritization    | Admin, Assessor | Score-sorted demand ranking (Requesters see access denied) |
| `/capacity`              | Capacity          | Admin, Assessor | Resource capacity planning (Requesters see access denied) |

---

## Features

### Demand Lifecycle

1. **Submit** - Requester creates a demand with title, problem statement, type, urgency, estimated effort, target date, and optional file attachments.
2. **Review** - Admin/Assessor reviews the demand. May request additional information from the requester with a specific message describing what is needed.
3. **Assess** - Admin/Assessor scores the demand on 5 criteria (1-5 each) and provides NPV financial analysis inputs. System auto-calculates weighted score (0-100) and NPV.
4. **Approve** - Admin/Assessor makes an approval decision (Approved/Rejected/On Hold) with comments. Decision syncs to the demand status.
5. **Track** - All actions are recorded as timeline events for full audit trail visibility.

### SLA Tracking

- Demands can have a **Target Date** (estimated finish date).
- Color-coded SLA badges show status:
  - **Red (Overdue)** - Past the target date
  - **Amber (Warning)** - Within 7 days of target date
  - **Green (OK)** - More than 7 days remaining

### Request Additional Information

- Admin/Assessor can click "Request Info" and type a specific message describing what information is needed.
- The demand status changes to `NeedsInfo`.
- The requester sees:
  - A warning banner on the Demands list page
  - An "Action Required" badge on the demand row
  - The specific info request message on the demand detail page
  - An edit form to update their demand and provide the requested information

### File Attachments

- Upload up to 5 files per demand at creation time.
- Supported formats: PDF, Word, Excel, PowerPoint, images, text, CSV, ZIP.
- Max file size: 10 MB per file.
- Files stored on disk with GUID-based filenames to prevent collisions.
- Download and delete capabilities on the demand detail page.

### Activity Timeline

Every significant action creates a `DemandEvent` entry:
- Created, Updated, Assessed, Approved, Rejected, OnHold
- InfoRequested (with admin's message), NoteAdded, FileUploaded

Events are displayed chronologically with color-coded timeline dots.

### Resource & Capacity Planning

- **Resources** - Manage a pool of team members with roles, departments, and monthly capacity hours.
- **Allocations** - Assign resources to demands with specific hour allocations per month.
- **Forecast** - View a 6-month rolling capacity forecast showing available vs. allocated hours by department.

### Dashboard

- KPI summary cards (Total, Intake, Under Review, Awaiting Approval)
- High-urgency demand highlights
- Top 5 priorities by weighted score
- Role-aware welcome message describing each role's capabilities

### Demand Ownership & Data Isolation

- **Server-side enforcement**: The API filters demands by `RequestedBy` for Requester role users. Requesters only receive their own demands from `GET /api/demands`.
- **Detail access control**: `GET /api/demands/{id}` returns `403 Forbidden` if a Requester attempts to access a demand that doesn't belong to them.
- **UI reinforcement**: The Demands page shows "View and manage your submitted demand requests" for Requesters vs. "View and manage all demand requests" for Admin/Assessor.

### Navigation & Access Control

- **Dynamic navigation**: The sidebar hides Prioritization and Capacity Planning links for Requester users.
- **Access denied pages**: If a Requester navigates directly to `/prioritization` or `/capacity`, they see a styled "Access Restricted" message explaining the page is for Admin/Assessor roles, with a redirect link to their demands.
- **Contextual messaging**: Each page shows role-appropriate descriptions and instructions.

---

## Configuration

### API (`DemandManagement2.Api/appsettings.json`)

```json
{
  "JwtSettings": {
    "SecretKey": "<your-secret-key-min-32-bytes>",
    "Issuer": "DemandManagement2.Api",
    "Audience": "DemandManagement2.Ui",
    "ExpirationMinutes": 480
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\sqlexpress;Database=DemandManagementDatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### UI (`DemandManagement2.Ui/appsettings.json`)

```json
{
  "ApiBaseUrl": "http://localhost:5182"
}
```

> **Note:** For production, change the JWT secret key, use HTTPS, and configure proper CORS policies.

---

## Testing

The test project includes unit tests for the scoring and NPV calculation logic.

```bash
cd DemandManagement2.Tests
dotnet test
```

### Test Coverage

| Test Class              | Tests | Description                                    |
|-------------------------|-------|------------------------------------------------|
| `ScoringServiceTests`  | 9     | Weighted score algorithm validation            |
| `NpvCalculationTests`  | 7     | Net Present Value calculation verification     |

**Total: 16 tests**

---

## Database Migrations

Migrations are managed via EF Core. To apply or update:

```bash
cd DemandManagement2.Api
dotnet ef database update
```

| Migration                        | Description                                    |
|----------------------------------|------------------------------------------------|
| `InitialCreate`                  | Base schema (DemandRequest, Assessment, Approval) |
| `AddResourcesAndDecisionNotes`   | Resource pool and decision notes               |
| `AddNPVFields`                   | NPV financial analysis fields on Assessment    |
| `AddUsers`                       | User entity with email/password/role           |
| `AddSlaTimelineAttachments`      | TargetDate, DemandEvent, DemandAttachment      |

---

## Ports

| Service | URL                       |
|---------|---------------------------|
| API     | `http://localhost:5182`    |
| UI      | `http://localhost:5235`    |
| Swagger | `http://localhost:5182/swagger` |
