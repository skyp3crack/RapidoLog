<div align="center">

# 🚀 RapidoLog

### High-Concurrency .NET 10 Logistics & Fintech Orchestrator

![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C# 14](https://img.shields.io/badge/C%23-14-239120?style=for-the-badge&logo=csharp&logoColor=white)
![Python](https://img.shields.io/badge/Python-3.12+-3776AB?style=for-the-badge&logo=python&logoColor=white)
![FastAPI](https://img.shields.io/badge/FastAPI-0.115+-009688?style=for-the-badge&logo=fastapi&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-yellow?style=for-the-badge)

**A resilient, distributed logistics orchestrator integrating simulated PayNet/DuitNow payment gateways and an AI-driven data extraction microservice — engineered for the Malaysian fintech & logistics ecosystem.**

[Architecture](#-system-architecture) · [Tech Stack](#-tech-stack) · [Getting Started](#-getting-started) · [API Reference](#-api-endpoints) · [Compliance](#-enterprise--compliance-features)

</div>

---

## 📋 Project Overview

**RapidoLog** is a production-grade logistics orchestration platform that demonstrates enterprise-level distributed systems design within the Malaysian financial infrastructure. The system coordinates shipment lifecycle management with real-time payment processing through simulated **PayNet/DuitNow** gateways — Malaysia's national payment network.

### What It Solves

In Malaysian logistics, a shipment cannot be dispatched until payment is confirmed by a banking authority. This creates a **distributed transaction problem** across two independent systems (logistics + payments). RapidoLog solves this with the **Saga Design Pattern**, orchestrating the following end-to-end flow:

```
Client Request → AI Address Extraction → Shipment Creation → Payment Initiation → Bank Webhook → Saga Resolution → Dispatch
```

### Key Highlights

- 🏗️ **Clean Architecture** with strict dependency inversion across four decoupled layers
- 📨 **CQRS with MediatR** — Commands and queries are fully separated through a mediator pipeline
- 🔄 **Saga Orchestration** — Distributed transaction management across shipment + payment bounded contexts
- 🛡️ **Polly Resilience** — Exponential backoff retry policies for fault-tolerant payment gateway communication
- 🤖 **AI Microservice** — Google Gemini-powered NLP engine for Malaysian address parsing (Malay/English)
- 🏦 **Fintech Integration** — Simulated PayNet/DuitNow FPX sandbox for realistic payment lifecycle testing

---

## 🏗️ System Architecture

RapidoLog follows **Clean Architecture** principles with strict layer separation. Dependencies flow inward — outer layers depend on inner layers, never the reverse.

```
┌──────────────────────────────────────────────────────────────────────┐
│                        API Layer (Presentation)                      │
│                   .NET 10 Minimal APIs · Program.cs                  │
│              POST /api/shipments  ·  POST /api/webhooks/paynet       │
└──────────────────────────┬───────────────────────────────────────────┘
                           │  MediatR Pipeline
                           ▼
┌──────────────────────────────────────────────────────────────────────┐
│                       Application Layer (CQRS)                       │
│          CreateShipmentCommand  ·  ProcessPaymentWebHookCommand      │
│               Interfaces: IAppDbContext · IPayNetService             │
│                          IAiRoutingService                           │
└──────────────────────────┬───────────────────────────────────────────┘
                           │  Dependency Inversion
                           ▼
┌──────────────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer (I/O)                       │
│        AppDbContext (EF Core)  ·  PayNetSimulatorService (Polly)     │
│                  AiRoutingService (HttpClient → FastAPI)             │
│              DependencyInjection.cs · Migrations                     │
└──────────────────────────┬───────────────────────────────────────────┘
                           │
                           ▼
┌──────────────────────────────────────────────────────────────────────┐
│                        Domain Layer (Core)                           │
│           Entities: Shipment · PaymentTransaction                    │
│           Enums: ShipmentStatus · PaymentStatus                      │
│            Rich Domain Model with Business Invariants                │
└──────────────────────────────────────────────────────────────────────┘

                    ┌──────────────────────┐
                    │   AI Microservice    │
                    │  Python · FastAPI    │
                    │  Google Gemini 2.5   │
                    │  /api/extract-address│
                    └──────────────────────┘
```

### Saga Flow — Distributed Transaction Lifecycle

```
 ┌─────────┐       ┌─────────────┐       ┌────────────┐       ┌──────────┐
 │  Client  │──1──▶│ Create      │──2──▶│ AI Service  │──3──▶│ PayNet    │
 │  (POST)  │      │ Shipment    │      │ (Gemini)    │      │ Gateway   │
 └─────────┘       └─────────────┘      └────────────┘       └────┬─────┘
                                                                   │
        ┌──────────────────────────────────────────────────────────┘
        │ 4. Async Webhook Callback
        ▼
 ┌─────────────┐       ┌──────────────────────────────────────────────┐
 │  Webhook    │──5──▶│              SAGA ENGINE                      │
 │  Handler    │      │  ✅ SUCCESS → MarkAsPaid → ReadyForDispatch   │
 │  (POST)     │      │  ❌ FAILED  → MarkAsFailed → CancelShipment  │
 └─────────────┘      └──────────────────────────────────────────────┘
```

---

## ⚙️ Tech Stack

### Backend — .NET Orchestrator

| Technology                     | Version   | Purpose                                          |
| ------------------------------ | --------- | ------------------------------------------------ |
| **.NET**                       | 10.0      | Runtime & Minimal API framework                  |
| **C#**                         | 14        | Primary language with modern syntax features      |
| **Entity Framework Core**      | 10.0.8    | ORM with code-first migrations & Fluent API      |
| **MediatR**                    | 14.1.0    | CQRS mediator pipeline for command/query dispatch |
| **Polly**                      | 8.6.6     | Resilience & transient-fault-handling (retry, circuit breaker) |
| **SQL Server**                 | 2022      | Relational persistence (LocalDB / Express)       |
| **Microsoft.Extensions.Http**  | 10.0.8    | Typed `HttpClient` factory for microservice calls |

### AI Microservice — Python

| Technology       | Purpose                                              |
| ---------------- | ---------------------------------------------------- |
| **FastAPI**      | High-performance async REST framework                |
| **Pydantic v2**  | Schema validation & structured JSON output           |
| **Google Gemini 2.5 Flash** | LLM-powered Malaysian address extraction |
| **Uvicorn**      | ASGI production server                               |
| **python-dotenv**| Secure environment variable management               |

### Architecture Patterns

| Pattern                    | Implementation                                            |
| -------------------------- | --------------------------------------------------------- |
| **Clean Architecture**     | 4-layer separation: Domain → Application → Infrastructure → API |
| **CQRS**                   | Commands separated via MediatR `IRequest<T>` pipeline     |
| **Saga (Orchestration)**   | `ProcessPaymentWebHookCommandHandler` manages distributed tx |
| **Repository (Implicit)**  | `IAppDbContext` abstracts EF Core `DbSet<T>` access       |
| **Dependency Inversion**   | All contracts defined in Application; implemented in Infrastructure |
| **Rich Domain Model**      | Entities enforce business invariants via encapsulated methods |

---

## 🏦 Enterprise & Compliance Features

### Bank Negara Malaysia (BNM) RMiT Alignment

RapidoLog is designed with awareness of Malaysia's **Risk Management in Technology (RMiT)** regulatory framework issued by Bank Negara Malaysia. The architecture supports the following compliance vectors:

| RMiT Domain                        | Implementation                                             |
| ----------------------------------- | ---------------------------------------------------------- |
| **Resilience & Availability**       | Polly retry policies with exponential backoff (2s → 4s → 8s) for payment gateway calls |
| **Transaction Integrity**           | Saga pattern with idempotency checks preventing duplicate webhook processing |
| **Audit Trail Readiness**           | Every `PaymentTransaction` is persisted with a unique `PayNetReference` for reconciliation |
| **Separation of Concerns**          | Clean Architecture enforces strict boundary between business logic and infrastructure |

### Security & Cryptography Readiness

| Capability                              | Status         | Details                                         |
| --------------------------------------- | -------------- | ----------------------------------------------- |
| **Azure Key Vault (HYOK)**              | 🟡 Architecture-Ready | Infrastructure layer designed for secret injection via `IConfiguration` |
| **Post-Quantum Cryptography (PQC)**     | 🟡 Architecture-Ready | Clean separation allows drop-in PQC transport layers |
| **Environment Variable Isolation**      | ✅ Implemented  | `.env` files with `.gitignore` exclusion for API keys |
| **Connection String Security**          | ✅ Implemented  | Externalized via `appsettings.json` configuration binding |

---

## 🚀 Getting Started

### Prerequisites

| Requirement        | Version    | Install                                               |
| ------------------ | ---------- | ----------------------------------------------------- |
| .NET SDK           | 10.0+      | [dotnet.microsoft.com](https://dotnet.microsoft.com)   |
| Python             | 3.12+      | [python.org](https://www.python.org)                   |
| SQL Server         | Express+   | [SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) |
| Google Gemini Key  | —          | [Google AI Studio](https://aistudio.google.com)        |

### 1. Clone the Repository

```bash
git clone https://github.com/skyp3crack/RapidoLog.git
cd RapidoLog
```

### 2. Configure the Database

Update the connection string in `RapidoLog.Api/appsettings.json` to match your SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RapidoLogDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### 3. Apply EF Core Migrations

```bash
dotnet ef database update --project RapidoLog.Infrastructure --startup-project RapidoLog.Api
```

### 4. Run the .NET API

```bash
cd RapidoLog.Api
dotnet watch
```

> The API will start on `http://localhost:5000` (or the port configured in `launchSettings.json`).

### 5. Set Up the AI Microservice

```bash
cd RapidoLog.AiService

# Create and activate virtual environment
python -m venv .venv
.venv\Scripts\activate        # Windows
# source .venv/bin/activate   # macOS/Linux

# Install dependencies
pip install -r requirements.txt
```

### 6. Configure the Gemini API Key

Create a `.env` file in the `RapidoLog.AiService/` directory:

```env
GEMINI_API_KEY=your_google_gemini_api_key_here
```

### 7. Run the AI Microservice

```bash
uvicorn main:app --host 127.0.0.1 --port 8000 --reload
```

> The FastAPI server will be available at `http://127.0.0.1:8000` with interactive docs at `/docs`.

---

## 📡 API Endpoints

### .NET Orchestrator API

| Method | Endpoint                  | Description                                             | Request Body                                                                 |
| ------ | ------------------------- | ------------------------------------------------------- | ---------------------------------------------------------------------------- |
| `POST` | `/api/shipments`          | Create a new shipment & initiate payment                | `{ "origin": "string", "destination": "string", "tenantId": "guid" }`       |
| `POST` | `/api/webhooks/paynet`    | Receive PayNet/DuitNow async payment webhook            | `{ "payNetReference": "string", "status": "SUCCESS \| FAILED" }`            |

### Python AI Microservice

| Method | Endpoint                  | Description                                             | Request Body                        |
| ------ | ------------------------- | ------------------------------------------------------- | ----------------------------------- |
| `POST` | `/api/extract-address`    | Extract structured address from raw Malaysian text      | `{ "rawAddress": "string" }`        |

### Example — Create Shipment

```bash
curl -X POST http://localhost:5000/api/shipments \
  -H "Content-Type: application/json" \
  -d '{
    "origin": "Kuala Lumpur",
    "destination": "hantar ke blok A-12-3 kondominium seri maya, jln pjs 10/32, 46150 petaling jaya selangor",
    "tenantId": "d3b07384-d9a0-4e9a-8f1a-1b0c5e8b9a7f"
  }'
```

**Response:**

```json
{
  "message": "Shipment created successfully. Pending Payment.",
  "payNetUrl": "https://sandbox.duitnow.my/checkout?ref=PAYNET-638123456789&amt=15.00"
}
```

### Example — PayNet Webhook Callback

```bash
curl -X POST http://localhost:5000/api/webhooks/paynet \
  -H "Content-Type: application/json" \
  -d '{
    "payNetReference": "PAYNET-638123456789",
    "status": "SUCCESS"
  }'
```

**Response:**

```json
{
  "status": "Webhook processed and Saga updated."
}
```

---

## 📁 Project Structure

```
RapidoLog/
├── RapidoLog.Api/                    # Presentation Layer — Minimal API endpoints
│   ├── Program.cs                    # App bootstrap, DI wiring, route mapping
│   ├── appsettings.json              # Configuration & connection strings
│   └── RapidoLog.Api.csproj
│
├── RapidoLog.Application/            # Application Layer — CQRS Commands & Interfaces
│   ├── Common/
│   │   └── Interfaces/
│   │       ├── IAppDbContext.cs       # Database abstraction contract
│   │       ├── IPayNetService.cs      # Payment gateway contract
│   │       └── IAiRoutingService.cs   # AI extraction contract
│   ├── Shipments/
│   │   └── Commands/
│   │       ├── CreateShipmentCommand.cs            # Shipment creation + payment initiation
│   │       └── ProcessPaymentWebHookCommand.cs     # Saga engine — webhook resolution
│   └── RapidoLog.Application.csproj
│
├── RapidoLog.Domain/                 # Domain Layer — Entities & Business Rules
│   ├── Entities/
│   │   ├── Shipment.cs               # Rich domain model with state transitions
│   │   └── PaymentTransaction.cs     # Payment lifecycle tracking
│   ├── Enums/
│   │   ├── ShipmentStatus.cs         # PendingPayment → Paid → ReadyForDispatch → Dispatched
│   │   └── PaymentStatus.cs          # Pending → Success | Failed
│   └── RapidoLog.Domain.csproj
│
├── RapidoLog.Infrastructure/         # Infrastructure Layer — External I/O
│   ├── Persistence/
│   │   └── AppDbContext.cs           # EF Core context with Fluent API config
│   ├── Services/
│   │   ├── PayNetSimulatorService.cs # Simulated PayNet gateway with Polly retry
│   │   └── AiRoutingService.cs       # HTTP client to Python AI microservice
│   ├── Migrations/                   # EF Core code-first migrations
│   ├── DependencyInjection.cs        # Service registration & DI wiring
│   └── RapidoLog.Infrastructure.csproj
│
├── RapidoLog.AiService/             # AI Microservice — Python/FastAPI
│   ├── main.py                       # Gemini-powered Malaysian address parser
│   ├── requirements.txt              # Python dependencies
│   └── .env                          # API key configuration (gitignored)
│
├── RapidoLog.slnx                   # .NET Solution file (XML format)
└── LICENSE                          # MIT License
```

---

## 🗺️ Roadmap

- [ ] **Event Sourcing** — Full event replay capability for shipment state transitions
- [ ] **Azure Service Bus** — Replace synchronous webhook with async message queues
- [ ] **Docker Compose** — Containerized multi-service deployment
- [ ] **OpenTelemetry** — Distributed tracing across .NET and Python services
- [ ] **Azure Key Vault Integration** — Production-grade HYOK secret management
- [ ] **Rate Limiting & API Gateway** — YARP or Azure API Management
- [ ] **Post-Quantum TLS** — ML-KEM transport layer for payment channels

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

---

<div align="center">

**Built with 🇲🇾 for the Malaysian Fintech & Logistics Ecosystem**

*Engineered to demonstrate enterprise-grade distributed systems design, regulatory awareness, and production-level software craftsmanship.*

</div>
