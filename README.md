# Demand Management Application

A full-stack demand management system built with **.NET 10.0**, **Blazor Server**, and **ASP.NET Core REST API**. The application enables organisations to submit, assess, prioritise, approve, and track IT demand requests with role-based access control, financial analysis (NPV), resource and capacity planning, budget tracking, reporting, email notifications, and full audit trails.

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
- [Database Migrations](#database-migrations)
- [Testing](#testing)
- [Ports](#ports)

---

## Architecture Overview

```
DemandManagement2.Ui  (Blazor Server — Port 5235)
        |
        |  HTTP/REST + JWT Bearer
        v
DemandManagement2.Api  (ASP.NET Core Web API — Port 5182)
        |
        |  Entity Framework Core
        v
DemandManagement2.Infrastructure  (Data Access)
        |
        v
DemandManagement2.Domain  (Entities & Enums)
```

- **UI** communicates with the **API** over HTTP using JWT Bearer tokens stored in `ProtectedSessionStorage`.
- **API** uses **Infrastructure** (EF Core + SQL Server) for data persistence.
- **Domain** contains all entity models and enums shared across layers.
- Prerendering is **disabled** (`prerender: false`) — all rendering is handled by the Blazor SignalR circuit.

---

## Technology Stack

| Component          | Technology                              |
|--------------------|-----------------------------------------|
| Framework          | .NET 10.0                               |
| Frontend           | Blazor Server (InteractiveServer)       |
| Backend            | ASP.NET Core Web API                    |
| Database           | SQL Server (LocalDB / SQL Express)      |
| ORM                | Entity Framework Core 10.0              |
| Authentication     | JWT Bearer Tokens                       |
| Password Hashing   | BCrypt.Net-Next                         |
| Email              | MailKit / MimeKit                       |
| Excel Export       | ClosedXML                               |
| PDF Export         | QuestPDF                                |
| Charts             | Blazor-ApexCharts                       |
| API Docs           | Swagger / Swashbuckle                   |
| Testing            | xUnit                                   |

---

## Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or SQL Express)
- SMTP server (optional — for email notifications)

### Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DemandManagementApp2
   ```

2. **Configure the API** — edit `DemandManagement2.Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\sqlexpress;Database=DemandManagementDatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
     },
     "JwtSettings": {
       "SecretKey": "<your-secret-key-min-32-bytes>",
       "Issuer": "DemandManagement2.Api",
       "Audience": "DemandManagement2.Ui",
       "ExpirationMinutes": 480
     },
     "Email": {
       "Enabled": false,
       "SmtpHost": "smtp.example.com",
       "SmtpPort": 587,
       "Username": "noreply@example.com",
       "Password": "<smtp-password>",
       "FromAddress": "noreply@example.com",
       "FromName": "Demand Management"
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
   dotnet run --project DemandManagement2.Api
   ```
   API starts on `http://localhost:5182`. Swagger UI: `http://localhost:5182/swagger`.

5. **Run the UI** (separate terminal)
   ```bash
   dotnet run --project DemandManagement2.Ui
   ```
   UI starts on `http://localhost:5235`.

6. **Register a user** at `/register` and start using the application.

---

## Project Structure

```
DemandManagementApp2/
├── DemandManagement2.Domain/           # Entities & enums
│   └── Entities/
│       ├── DemandRequest.cs            # Primary aggregate root
│       ├── Assessment.cs               # Scoring & NPV (CapEx + OpEx)
│       ├── ApprovalDecision.cs         # Approval workflow
│       ├── DecisionNote.cs             # Meeting notes
│       ├── DemandEvent.cs              # Activity timeline / audit trail
│       ├── DemandAttachment.cs         # File attachments
│       ├── BudgetEntry.cs              # Budget tracking entries
│       ├── Resource.cs                 # Team resource pool
│       ├── ResourceAllocation.cs       # Capacity allocations
│       ├── ProjectResource.cs          # Project resource requirements
│       └── User.cs                     # User accounts & roles
│
├── DemandManagement2.Infrastructure/   # Data access layer
│   └── Data/
│       ├── AppDbContext.cs             # EF Core DbContext
│       └── Migrations/                 # Database migrations
│
├── DemandManagement2.Api/              # REST API
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── DemandRequestsController.cs
│   │   ├── AssessmentsController.cs
│   │   ├── ApprovalsController.cs
│   │   ├── DecisionNotesController.cs
│   │   ├── AttachmentsController.cs
│   │   ├── PrioritizationController.cs
│   │   ├── DashboardController.cs
│   │   ├── ResourcesController.cs
│   │   ├── CapacityController.cs
│   │   ├── ProjectResourcesController.cs
│   │   ├── BudgetController.cs
│   │   └── ReportsController.cs
│   ├── Dtos/                           # Data transfer objects
│   └── Services/
│       ├── ScoringService.cs           # Weighted scoring algorithm
│       └── EmailService.cs             # MailKit SMTP notifications
│
├── DemandManagement2.Ui/               # Blazor Server frontend
│   ├── Components/
│   │   ├── App.razor                   # HTML shell & global scripts
│   │   ├── Pages/
│   │   │   ├── Home.razor
│   │   │   ├── Login.razor
│   │   │   ├── Register.razor
│   │   │   ├── Dashboard.razor
│   │   │   ├── Demands.razor
│   │   │   ├── DemandDetails.razor
│   │   │   ├── NewDemand.razor
│   │   │   ├── Prioritization.razor
│   │   │   ├── Capacity.razor          # Capacity + Project Resources
│   │   │   ├── Budget.razor
│   │   │   └── Reports.razor
│   │   └── Layout/
│   │       ├── NavMenu.razor
│   │       └── MainLayout.razor
│   └── Services/
│       ├── DemandApiClient.cs          # HTTP client for all API calls
│       ├── AuthService.cs
│       ├── JwtAuthenticationStateProvider.cs
│       └── TokenStorageService.cs
│
└── DemandManagement2.Tests/            # Unit tests (xUnit)
    ├── ScoringServiceTests.cs
    └── NpvCalculationTests.cs
```

---

## Authentication & Authorization

### Roles

| Role          | Description                                                                      |
|---------------|----------------------------------------------------------------------------------|
| **Admin**     | Full access: all demands, assess, approve, delete, reports, resources, budget    |
| **Assessor**  | Assess and approve demands, manage resources, capacity, and project resources    |
| **Requester** | Submit and track own demands only                                                |

### Role-Based Access Matrix

| Feature                        | Admin | Assessor | Requester       |
|--------------------------------|-------|----------|-----------------|
| Dashboard                      | Full  | Full     | Own demands only|
| View all demands               | Yes   | Yes      | No (own only)   |
| Submit new demand              | Yes   | Yes      | Yes             |
| Edit demand                    | Yes   | Yes      | Own (Intake/NeedsInfo) |
| Assess demand                  | Yes   | Yes      | No              |
| Approve / Reject demand        | Yes   | Yes      | No              |
| Delete demand                  | Yes   | Yes      | No              |
| Prioritization page            | Yes   | Yes      | Access denied   |
| Capacity Planning page         | Yes   | Yes      | Access denied   |
| Manage team resources          | Yes   | Yes      | No              |
| Manage project resources       | Yes   | Yes      | No              |
| Budget page                    | Yes   | Yes      | No              |
| Reports & Export               | Yes   | Yes      | No              |
| Download attachments           | Yes   | Yes      | Yes (own)       |

### Auth Flow

1. User registers at `/register` with name, email, password, and role.
2. User logs in at `/login` — API returns a JWT (8-hour expiry).
3. Token is stored in `ProtectedSessionStorage` (server-side encrypted).
4. `JwtAuthenticationStateProvider` decodes the JWT and exposes user claims.
5. `DemandApiClient` attaches the Bearer token to every API request.
6. API endpoints enforce role restrictions via `[Authorize(Roles = "...")]`.
7. Requesters receive only their own demands (server-side filtering).

---

## Domain Model

### Entities & Relationships

```
User (Guid Id, FullName, Email, PasswordHash, Role)

DemandRequest (Guid Id, Title, ProblemStatement, Type, Status, TargetDate, ...)
  ├── Assessment (1:1)         — Scoring, NPV, CapExAmount, OpExAmount
  ├── ApprovalDecision (1:1)   — Approve / Reject / OnHold
  ├── DecisionNote (1:N)       — Meeting notes
  ├── DemandEvent (1:N)        — Activity timeline / audit trail
  ├── DemandAttachment (1:N)   — File attachments
  ├── ResourceAllocation (1:N) — Team resource assignments
  └── ProjectResource (1:N)    — Financial, Physical, Information & Governance requirements

Resource (Guid Id, Name, Role, Department, CapacityHoursPerMonth)
  └── ResourceAllocation (1:N)

BudgetEntry (Guid Id, Title, Amount, Category, EntryDate, ...)
```

### Demand Statuses

| Status         | Description                                          |
|----------------|------------------------------------------------------|
| `Intake`       | Newly submitted, awaiting review                     |
| `UnderReview`  | Assessment in progress                               |
| `Prioritized`  | Scored and ranked                                    |
| `Approved`     | Approved for implementation                          |
| `Backlog`      | On hold / deferred                                   |
| `Rejected`     | Not approved                                         |
| `NeedsInfo`    | Additional information requested from the submitter  |

### Demand Types

| Type              | Description                        |
|-------------------|------------------------------------|
| `Project`         | New project initiative             |
| `Enhancement`     | Improvement to existing system     |
| `Service`         | Service request                    |
| `ResourceRequest` | Request for additional resources   |

### Project Resource Types

| Type                    | Examples                                              |
|-------------------------|-------------------------------------------------------|
| `Financial`             | Software licensing, contractor fees, training costs   |
| `Physical`              | Laptops, servers, network equipment                   |
| `InformationKnowledge`  | Requirements docs, process maps, business data        |
| `GovernanceSupport`     | Risk frameworks, change management, QA standards      |

### Scoring Algorithm

Demands are scored on a 0–100 scale using weighted criteria:

| Criterion            | Weight | Direction                  |
|----------------------|--------|----------------------------|
| Business Value       | 30%    | Higher is better           |
| Strategic Alignment  | 25%    | Higher is better           |
| Urgency              | 15%    | Higher is better           |
| Cost Impact          | 10%    | Lower is better (inverted) |
| Risk                 | 10%    | Lower is better (inverted) |
| Resource Need        | 10%    | Lower is better (inverted) |

### NPV Calculation

```
NPV = -InitialCost + SUM( AnnualBenefit / (1 + DiscountRate)^t )   for t = 1..ProjectYears
```

Assessment also captures **CapEx** (capital expenditure) and **OpEx** (operational expenditure) fields separately.

---

## API Reference

Base URL: `http://localhost:5182`

### Auth — `/api/auth`

| Method | Endpoint             | Auth   | Description         |
|--------|----------------------|--------|---------------------|
| POST   | `/api/auth/register` | Public | Register a new user |
| POST   | `/api/auth/login`    | Public | Login, receive JWT  |

### Demands — `/api/demands`

| Method | Endpoint                          | Auth            | Description                                 |
|--------|-----------------------------------|-----------------|---------------------------------------------|
| GET    | `/api/demands`                    | Authenticated   | List demands (Requesters: own only)         |
| GET    | `/api/demands/{id}`               | Authenticated   | Get demand details                          |
| POST   | `/api/demands`                    | Authenticated   | Create a new demand                         |
| PUT    | `/api/demands/{id}`               | Authenticated   | Update demand                               |
| PATCH  | `/api/demands/{id}/request-info`  | Admin, Assessor | Request additional info from submitter      |
| DELETE | `/api/demands/{id}`               | Admin, Assessor | Delete demand and all related data          |

### Assessments — `/api/demands/{demandId}/assessment`

| Method | Endpoint                                  | Auth            | Description                               |
|--------|-------------------------------------------|-----------------|-------------------------------------------|
| GET    | `/api/demands/{demandId}/assessment`      | Admin, Assessor | Get assessment                            |
| POST   | `/api/demands/{demandId}/assessment`      | Admin, Assessor | Create/update assessment (auto-calculates NPV & score) |

### Approvals — `/api/demands/{demandId}/approval`

| Method | Endpoint                                  | Auth            | Description                               |
|--------|-------------------------------------------|-----------------|-------------------------------------------|
| GET    | `/api/demands/{demandId}/approval`        | Admin, Assessor | Get approval decision                     |
| POST   | `/api/demands/{demandId}/approval`        | Admin, Assessor | Create/update approval (syncs demand status) |

### Decision Notes — `/api/demands/{demandId}/notes`

| Method | Endpoint                                     | Auth          | Description      |
|--------|----------------------------------------------|---------------|------------------|
| GET    | `/api/demands/{demandId}/notes`              | Authenticated | List all notes   |
| POST   | `/api/demands/{demandId}/notes`              | Authenticated | Create note      |
| PUT    | `/api/demands/{demandId}/notes/{id}`         | Authenticated | Update note      |
| DELETE | `/api/demands/{demandId}/notes/{id}`         | Authenticated | Delete note      |

### Attachments — `/api/demands/{demandId}/attachments`

| Method | Endpoint                                               | Auth            | Description              |
|--------|--------------------------------------------------------|-----------------|--------------------------|
| POST   | `/api/demands/{demandId}/attachments`                  | Authenticated   | Upload file (max 10 MB)  |
| GET    | `/api/demands/{demandId}/attachments/{attachmentId}`   | Public          | Download file            |
| DELETE | `/api/demands/{demandId}/attachments/{attachmentId}`   | Admin, Assessor | Delete file              |

**Allowed types:** `.pdf`, `.doc`, `.docx`, `.xls`, `.xlsx`, `.ppt`, `.pptx`, `.png`, `.jpg`, `.jpeg`, `.txt`, `.csv`, `.zip`

### Prioritization — `/api/prioritization`

| Method | Endpoint              | Auth          | Description                                    |
|--------|-----------------------|---------------|------------------------------------------------|
| GET    | `/api/prioritization` | Authenticated | Demands sorted by WeightedScore, Urgency, Date |

### Dashboard — `/api/dashboard`

| Method | Endpoint                  | Auth          | Description                          |
|--------|---------------------------|---------------|--------------------------------------|
| GET    | `/api/dashboard/summary`  | Authenticated | Total count and status breakdown     |
| GET    | `/api/dashboard/aging`    | Authenticated | Demand aging and overdue flags       |

### Resources — `/api/resources`

| Method | Endpoint              | Auth            | Description                           |
|--------|-----------------------|-----------------|---------------------------------------|
| GET    | `/api/resources`      | Admin, Assessor | List all active team resources        |
| POST   | `/api/resources`      | Admin, Assessor | Create resource                       |
| PUT    | `/api/resources/{id}` | Admin, Assessor | Update resource                       |
| DELETE | `/api/resources/{id}` | Admin, Assessor | Soft-delete resource (IsActive=false) |

### Capacity — `/api/capacity`

| Method | Endpoint                         | Auth          | Description                      |
|--------|----------------------------------|---------------|----------------------------------|
| GET    | `/api/capacity/summary`          | Authenticated | Monthly capacity by department   |
| GET    | `/api/capacity/allocations`      | Authenticated | Resource allocations for month   |
| POST   | `/api/capacity/allocations`      | Authenticated | Create allocation                |
| DELETE | `/api/capacity/allocations/{id}` | Authenticated | Delete allocation                |
| GET    | `/api/capacity/forecast`         | Authenticated | 6-month capacity forecast        |

### Project Resources — `/api/project-resources`

| Method | Endpoint                        | Auth            | Description                                         |
|--------|---------------------------------|-----------------|-----------------------------------------------------|
| GET    | `/api/project-resources`        | Authenticated   | List all project resources (filter: `?demandId=`)  |
| GET    | `/api/project-resources/{id}`   | Authenticated   | Get single project resource                         |
| POST   | `/api/project-resources`        | Admin, Assessor | Create project resource                             |
| PUT    | `/api/project-resources/{id}`   | Admin, Assessor | Update project resource                             |
| DELETE | `/api/project-resources/{id}`   | Admin, Assessor | Delete project resource                             |
| POST   | `/api/project-resources/reorder`| Admin, Assessor | Bulk-update sort order (drag-and-drop persistence)  |

### Budget — `/api/budget`

| Method | Endpoint            | Auth            | Description            |
|--------|---------------------|-----------------|------------------------|
| GET    | `/api/budget`       | Admin, Assessor | List budget entries    |
| POST   | `/api/budget`       | Admin, Assessor | Create budget entry    |
| PUT    | `/api/budget/{id}`  | Admin, Assessor | Update budget entry    |
| DELETE | `/api/budget/{id}`  | Admin, Assessor | Delete budget entry    |

### Reports — `/api/reports`

| Method | Endpoint                  | Auth            | Description                      |
|--------|---------------------------|-----------------|----------------------------------|
| GET    | `/api/reports/csv`        | Admin, Assessor | Export demands as CSV            |
| GET    | `/api/reports/excel`      | Admin, Assessor | Export demands as Excel (.xlsx)  |
| GET    | `/api/reports/pdf`        | Admin, Assessor | Export demands as PDF            |

---

## UI Pages

| Route             | Page              | Access          | Description                                                 |
|-------------------|-------------------|-----------------|-------------------------------------------------------------|
| `/`               | Home              | Public          | Landing page with overview and navigation                   |
| `/login`          | Login             | Public          | Email / password login                                      |
| `/register`       | Register          | Public          | User registration with role selection                       |
| `/dashboard`      | Dashboard         | Authenticated   | KPI cards, highlights, top priorities (role-aware)          |
| `/demands`        | Demands           | Authenticated   | Demand list (Requesters: own only)                          |
| `/demands/new`    | New Demand        | Authenticated   | Create demand form with file upload                         |
| `/demands/{id}`   | Demand Details    | Authenticated   | Full detail: assessment, approval, notes, attachments, timeline |
| `/prioritization` | Prioritization    | Admin, Assessor | Score-sorted demand ranking                                 |
| `/capacity`       | Capacity Planning | Admin, Assessor | Team resources, allocations, and project resource cards     |
| `/budget`         | Budget Tracking   | Admin, Assessor | Budget entries with bar, donut, and line charts             |
| `/reports`        | Reports & Export  | Admin, Assessor | Export demands as CSV, Excel, or PDF                        |

---

## Features

### Demand Lifecycle

1. **Submit** — Requester creates a demand with title, problem statement, type, urgency, estimated effort, target date, and optional file attachments.
2. **Review** — Admin/Assessor reviews. May request additional information with a specific message; demand moves to `NeedsInfo`.
3. **Assess** — Admin/Assessor scores 5 criteria (1–5), provides NPV inputs (CapEx, OpEx, annual benefit, discount rate, project years). System auto-calculates weighted score and NPV.
4. **Approve** — Admin/Assessor approves, rejects, or places on hold with comments. Decision syncs to demand status.
5. **Track** — All actions are recorded as timeline events (`DemandEvent`) for full audit trail visibility.

### SLA Tracking

Demands with a **Target Date** show a color-coded SLA badge:

| Badge  | Meaning                              |
|--------|--------------------------------------|
| Red    | Overdue — past the target date       |
| Amber  | Warning — within 7 days of due date  |
| Green  | OK — more than 7 days remaining      |

### Project Resources (Capacity Page)

A dedicated section on the Capacity Planning page allows Admin/Assessor to define and track all resource requirements for a demand:

- **Financial** — budgets, software licensing, contractor fees, training costs, vendor/supplier details.
- **Physical** — equipment requirements with quantity and vendor information.
- **Information & Knowledge** — business requirements documents, process maps, data assets, owner/responsible party.
- **Governance & Support** — risk frameworks, change management processes, QA standards, governance committees.

Cards can be **dragged and dropped** to reorder them. The new order is persisted to the database immediately.

### Budget Tracking

- Add budget entries by category (CapEx, OpEx, Training, Licensing, etc.).
- Dashboard charts powered by **ApexCharts**:
  - **Bar chart** — spending by category
  - **Donut chart** — budget distribution
  - **Line chart** — spending over time

### Reports & Export

Admins and Assessors can export the full demand list in three formats:

| Format    | Library     | Contents                                   |
|-----------|-------------|--------------------------------------------|
| CSV       | Built-in    | All demands with key fields                |
| Excel     | ClosedXML   | Formatted `.xlsx` with column headers      |
| PDF       | QuestPDF    | Styled PDF document with all demand data   |

### Email Notifications

Powered by **MailKit**. Emails are sent automatically for the following events:

| Event              | Recipient               | Trigger                                      |
|--------------------|-------------------------|----------------------------------------------|
| Demand Created     | All Admins & Assessors  | New demand submitted                         |
| Demand Assessed    | Requester               | Assessment scores saved                      |
| Demand Approved    | Requester               | Approval decision = Approved                 |
| Demand Rejected    | Requester               | Approval decision = Rejected                 |
| Demand On Hold     | Requester               | Approval decision = OnHold                   |
| Info Requested     | Requester               | Admin/Assessor requests additional info      |

To enable, update `DemandManagement2.Api/appsettings.json`:

```json
"EmailSettings": {
  "Enabled": true,
  "Host": "smtp.gmail.com",
  "Port": 587,
  "Username": "your-email@gmail.com",
  "Password": "<gmail-app-password>",
  "FromEmail": "your-email@gmail.com",
  "FromName": "Demand Management",
  "EnableSsl": true
}
```

> **Gmail tip:** Use an [App Password](https://myaccount.google.com/apppasswords) (not your account password). Enable 2-Factor Authentication on your Google account first, then generate an App Password for "Mail".

Set `"Enabled": false` to disable all notifications without removing the configuration.

### File Attachments

- Upload up to 5 files per demand at creation time.
- Max file size: **10 MB** per file.
- Stored on disk with GUID-based filenames to prevent collisions.
- Download and delete on the demand detail page.

### Activity Timeline

Every significant action creates a `DemandEvent` record:

`Created` · `Updated` · `Assessed` · `Approved` · `Rejected` · `OnHold` · `InfoRequested` · `NoteAdded` · `FileUploaded`

Events are displayed chronologically on the demand detail page with color-coded timeline dots.

### Resource & Capacity Planning

- **Team Resources** — manage a pool of team members with role, department, and monthly capacity hours.
- **Allocations** — assign team members to demands with specific hours per month.
- **Capacity Summary** — view allocated vs. available hours by department.
- **6-Month Forecast** — rolling capacity forecast chart.

---

## Configuration

### API — `DemandManagement2.Api/appsettings.json`

```json
{
  "JwtSettings": {
    "SecretKey": "<min-32-byte-secret>",
    "Issuer": "DemandManagement2.Api",
    "Audience": "DemandManagement2.Ui",
    "ExpirationMinutes": 480
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\sqlexpress;Database=DemandManagementDatabase;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  },
  "EmailSettings": {
    "Enabled": true,
    "Host": "smtp.gmail.com",
    "Port": 587,
    "Username": "your-email@gmail.com",
    "Password": "<your-app-password>",
    "FromEmail": "your-email@gmail.com",
    "FromName": "Demand Management",
    "EnableSsl": true
  }
}
```

### UI — `DemandManagement2.Ui/appsettings.json`

```json
{
  "ApiBaseUrl": "http://localhost:5182"
}
```

> **Production note:** Change the JWT secret, use HTTPS, and configure proper CORS policies before deploying.

---

## Database Migrations

```bash
cd DemandManagement2.Api
dotnet ef database update
```

| Migration                         | Description                                              |
|-----------------------------------|----------------------------------------------------------|
| `InitialCreate`                   | Base schema — DemandRequest, Assessment, Approval        |
| `AddResourcesAndDecisionNotes`    | Resource pool and decision notes                         |
| `AddNPVFields`                    | NPV financial fields on Assessment                       |
| `AddUsers`                        | User entity with email / password / role                 |
| `AddSlaTimelineAttachments`       | TargetDate, DemandEvent, DemandAttachment                |
| `AddBudgetTrackingFields`         | BudgetEntry, CapExAmount, OpExAmount on Assessment       |
| `AddProjectResources`             | ProjectResource entity with 4 resource types             |
| `AddProjectResourceSortOrder`     | SortOrder field for drag-and-drop card ordering          |

---

## Testing

```bash
cd DemandManagement2.Tests
dotnet test
```

| Test Class             | Tests | Description                         |
|------------------------|-------|-------------------------------------|
| `ScoringServiceTests`  | 9     | Weighted score algorithm validation |
| `NpvCalculationTests`  | 7     | Net Present Value calculation       |

**Total: 16 tests**

---

## Ports

| Service | URL                             |
|---------|---------------------------------|
| API     | `http://localhost:5182`         |
| UI      | `http://localhost:5235`         |
| Swagger | `http://localhost:5182/swagger` |
