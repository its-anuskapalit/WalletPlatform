# 🏦 Wallet & Payment Platform — Project Handoff Document

> **Last updated:** Phase 6 complete  
> **Stack:** .NET 8 · ASP.NET Core Web API · SQL Server Express · RabbitMQ · Ocelot · Angular 17  
> **Dev Machine:** Windows · Visual Studio 2022 Community  
> **Solution path:** `C:\Projects\WalletPlatform\`

---

## 📊 Progress Tracker

| Phase | Topic | Status |
|-------|-------|--------|
| Phase 1 | System Design & Architecture | ✅ Complete |
| Phase 2 | Environment Setup | ✅ Complete |
| Phase 3 | Authentication & User Service | ✅ Complete |
| Phase 4 | API Gateway (Ocelot) | ✅ Complete |
| Phase 5 | Wallet Service | ✅ Complete |
| Phase 6 | Transaction & Ledger Service | ✅ Complete |
| Phase 7 | Rewards & Loyalty Service | ⏳ Next |
| Phase 8 | Catalog & Redemption Service | ⏳ Pending |
| Phase 9 | Notification Service | ⏳ Pending |
| Phase 10 | Angular Frontend | ⏳ Pending |
| Phase 11 | Production Enhancements | ⏳ Pending |

---

## 🏗️ Architecture Overview

```
Angular Client (:4200)
        │  HTTPS
        ▼
API Gateway — Ocelot (:5000)
  JWT validation · Rate limiting · Circuit breaker (Polly)
        │  REST (sync)              │  Routes
        ▼                           ▼
┌─────────────────────────────────────────────────────┐
│                   Microservices                      │
│  Auth.API     :5010   wallet_db  → auth_db          │
│  Wallet.API   :5020   wallet_db                     │
│  Transaction  :5030   ledger_db                     │
│  Rewards      :5040   rewards_db                    │
│  Catalog      :5050   catalog_db                    │
│  Notification :5060   notification_db               │
│  Reporting    :5070   reporting_db                  │
└─────────────────────────────────────────────────────┘
        │  Async events
        ▼
   RabbitMQ (:5672 / UI :15672)
```

**Key rules:**
- Services **never** share a database or query each other's DB directly
- Cross-service data = REST API call OR RabbitMQ event subscription
- Gateway is the only component exposed to the internet
- JWT validated once at the gateway — services trust forwarded headers

---

## 🗄️ Database Reference

| Database | Owned By | Server |
|----------|----------|--------|
| `auth_db` | Auth.API | `POLLY\SQLEXPRESS` |
| `wallet_db` | Wallet.API | `POLLY\SQLEXPRESS` |
| `ledger_db` | Transaction.API | `POLLY\SQLEXPRESS` |
| `rewards_db` | Rewards.API | `POLLY\SQLEXPRESS` |
| `catalog_db` | Catalog.API | `POLLY\SQLEXPRESS` |
| `notification_db` | Notification.API | `POLLY\SQLEXPRESS` |
| `reporting_db` | Reporting.API | `POLLY\SQLEXPRESS` |

**Connection string template** (change `Initial Catalog` per service):
```
Data Source=POLLY\SQLEXPRESS;Initial Catalog=<db_name>;Integrated Security=True;Connect Timeout=30;Encrypt=True;TrustServerCertificate=True;
```

---

## 📦 Solution Structure

```
WalletPlatform/
├── src/
│   ├── Gateway/
│   │   └── ApiGateway/                  ← Ocelot gateway
│   ├── Services/
│   │   ├── Auth/Auth.API/               ← ✅ Built
│   │   ├── Wallet/Wallet.API/           ← ✅ Built
│   │   ├── Transaction/Transaction.API/ ← ✅ Built
│   │   ├── Rewards/Rewards.API/         ← ⏳ Next
│   │   ├── Catalog/Catalog.API/         ← ⏳ Pending
│   │   ├── Notification/Notification.API/ ← ⏳ Pending
│   │   └── Reporting/Reporting.API/     ← ⏳ Pending
│   └── Shared/
│       └── WalletPlatform.Shared/       ← ✅ Built — referenced by ALL services
├── frontend/
│   └── wallet-app/                      ← Angular 17, scaffolded only
├── docker/
│   └── docker-compose.yml               ← Not yet written
└── WalletPlatform.sln
```

---

## 🔌 Port Map

| Service | HTTP | HTTPS |
|---------|------|-------|
| ApiGateway | 5000 | 5001 |
| Auth.API | 5010 | 5011 |
| Wallet.API | 5020 | 5021 |
| Transaction.API | 5030 | 5031 |
| Rewards.API | 5040 | 5041 |
| Catalog.API | 5050 | 5051 |
| Notification.API | 5060 | 5061 |
| Reporting.API | 5070 | 5071 |

---

## 🔑 Shared Configuration Values

These values must be **identical** across every service's `appsettings.json`:

```json
"Jwt": {
  "Key": "WalletPlatform_SuperSecret_JWT_Key_2024_MustBe32CharsMin!!",
  "Issuer": "WalletPlatform.Auth",
  "Audience": "WalletPlatform.Client",
  "ExpiryMinutes": "60"
},
"RabbitMQ": {
  "HostName": "localhost",
  "UserName": "guest",
  "Password": "guest"
}
```

> ⚠️ If the JWT Key/Issuer/Audience differs between Gateway and any service, all tokens will be rejected with 401.

---

## 📨 RabbitMQ Event Contracts

All events are defined in `WalletPlatform.Shared/Events/`. Queue names are in `WalletPlatform.Shared/Constants/RabbitMQQueues.cs`.

| Queue Name | Event Class | Published By | Consumed By |
|------------|-------------|--------------|-------------|
| `user.registered` | `UserRegisteredEvent` | Auth | Wallet, Rewards, Notification |
| `kyc.approved` | `KYCApprovedEvent` | Auth | Wallet |
| `wallet.funded` | `WalletFundedEvent` | Wallet | Transaction, Notification |
| `payment.initiated` | `PaymentInitiatedEvent` | Wallet | Transaction |
| `transaction.completed` | `TransactionCompletedEvent` | Transaction | Rewards, Notification, Reporting |
| `transaction.failed` | `TransactionFailedEvent` | Transaction | Wallet (compensate), Notification |
| `points.awarded` | `PointsAwardedEvent` | Rewards | Notification |
| `redemption.requested` | `RedemptionRequestedEvent` | Catalog | Rewards, Notification |

**Every queue is configured with a Dead Letter Exchange (DLX):**
```csharp
arguments: new Dictionary<string, object>
{
    { "x-dead-letter-exchange", $"{queueName}.dlx" }
}
```

---

## ✅ Phase 3 — Auth Service (COMPLETE)

### What was built
- User registration with BCrypt password hashing
- JWT access token + refresh token (7-day rotation)
- Role-based authorization: `User`, `Admin`, `Agent`
- KYC workflow: Submit → Admin Review → Approve/Reject
- Audit logging on every auth action
- Publishes `UserRegisteredEvent` and `KYCApprovedEvent` to RabbitMQ
- FluentValidation on all request DTOs
- Global exception middleware

### Key files
```
Auth.API/
├── Entities/          User, UserProfile, KYCRecord, RefreshToken, AuditLog
├── Services/          AuthService, TokenService, KYCService
├── Repositories/      UserRepository, KYCRepository
├── Controllers/       AuthController, KYCController
├── Events/Publishers/ AuthEventPublisher
└── Middleware/        ExceptionMiddleware
```

### Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/auth/register` | Public | Register new user |
| POST | `/api/auth/login` | Public | Login, returns JWT |
| POST | `/api/auth/refresh-token` | Public | Rotate refresh token |
| POST | `/api/auth/logout` | JWT | Revoke refresh token |
| GET | `/api/auth/profile` | JWT | Get current user profile |
| POST | `/api/kyc/submit` | JWT | Submit KYC document |
| GET | `/api/kyc/status` | JWT | Get KYC status |
| GET | `/api/kyc/pending` | Admin | List pending KYC reviews |
| POST | `/api/kyc/{id}/approve` | Admin | Approve KYC |
| POST | `/api/kyc/{id}/reject` | Admin | Reject KYC with reason |

### Migration status
```bash
# Already run — tables exist in auth_db:
# Users, UserProfiles, KYCRecords, RefreshTokens, AuditLogs, __EFMigrationsHistory
```

---

## ✅ Phase 4 — API Gateway (COMPLETE)

### What was built
- Ocelot routing for all 7 services
- JWT validation at gateway level (services trust gateway)
- Rate limiting per route:
  - `/api/auth/register` → 5 req/min
  - `/api/auth/login` → 10 req/min
  - `/api/wallet/*` → 10 req/sec
  - `/api/transactions/*` → 5 req/sec
- Circuit breaker via Polly (3 failures → 10s open)
- CORS configured for Angular on `:4200`
- Health check proxy routes (`/health/auth`, `/health/wallet`, `/health/transaction`)
- `GatewayExceptionMiddleware` for clean 401/403/500 responses

### Key files
```
ApiGateway/
├── ocelot.json                ← Route definitions (Copy Always to output)
├── Middleware/                GatewayExceptionMiddleware
└── Program.cs
```

### Important note on ocelot.json
```
// Comments are NOT valid JSON — the file used in the session had
// explanatory comments. Remove all // lines before running.
// Set "Copy to Output Directory" = "Always" in file properties.
```

---

## ✅ Phase 5 — Wallet Service (COMPLETE)

### What was built
- Wallet auto-created when `UserRegisteredEvent` is consumed (no manual API call needed)
- Wallet starts in `Pending` status — activated automatically on `KYCApprovedEvent`
- Fund wallet, withdraw, freeze/unfreeze
- `AvailableBalance = Balance - FrozenAmount` (computed, not stored)
- Publishes `WalletFundedEvent` on top-up
- Admin-only freeze/unfreeze with audit log (`WalletFreezeLog`)
- `EnsureWalletOperable()` guard on every balance operation

### Wallet status flow
```
Pending → (KYC approved event) → Active
Active  → (Admin freeze)       → Frozen
Frozen  → (Admin unfreeze)     → Active
Active  → (Admin close)        → Closed
```

### Key files
```
Wallet.API/
├── Entities/           Wallet, PaymentMethod, WalletFreezeLog
├── Services/           WalletService
├── Repositories/       WalletRepository
├── Controllers/        WalletController
├── Events/Publishers/  WalletEventPublisher
└── Events/Consumers/   UserRegisteredConsumer, KYCApprovedConsumer
```

### Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| GET | `/api/wallet` | JWT | Get my wallet |
| POST | `/api/wallet/fund` | JWT | Add money to wallet |
| POST | `/api/wallet/withdraw` | JWT | Withdraw money |
| POST | `/api/wallet/{id}/freeze` | Admin | Freeze a wallet |
| POST | `/api/wallet/{id}/unfreeze` | Admin | Unfreeze a wallet |

### Migration status
```bash
# Already run — tables exist in wallet_db:
# Wallets, PaymentMethods, WalletFreezeLogs, __EFMigrationsHistory
```

---

## ✅ Phase 6 — Transaction & Ledger Service (COMPLETE)

### What was built
- Double-entry accounting: every payment creates exactly 2 ledger entries (1 DEBIT + 1 CREDIT)
- Immutable ledger: no UPDATEs or DELETEs ever on `LedgerEntries`
- Idempotency: `Idempotency-Key` header prevents duplicate payments
- Saga compensation: if ledger write fails → publishes `TransactionFailedEvent` → Wallet reverses debit
- Transaction status lifecycle: `Pending → Processing → Completed / Failed / Reversed`
- Balance derivation from ledger: `SUM(Credits) - SUM(Debits)` — acts as audit check vs wallet balance
- Consumes `WalletFundedEvent` to write top-up credit ledger entry

### Payment flow (9 steps)
```
1. Check idempotency key — return cached result if duplicate
2. Validate amount > 0, sender ≠ recipient
3. Create Transaction record (status: Pending)
4. Save idempotency record
5. Update status → Processing
6. Write 2 LedgerEntries atomically (DEBIT sender, CREDIT recipient)
7. Update status → Completed
8. Publish TransactionCompletedEvent
9. On any exception: update → Failed, publish TransactionFailedEvent (Saga)
```

### Key files
```
Transaction.API/
├── Entities/           Transaction, LedgerEntry, IdempotencyRecord
├── Services/           TransactionService, LedgerService
├── Repositories/       TransactionRepository, LedgerRepository
├── Controllers/        TransactionController
├── Events/Publishers/  TransactionEventPublisher
└── Events/Consumers/   WalletFundedConsumer
```

### Endpoints
| Method | Route | Auth | Description |
|--------|-------|------|-------------|
| POST | `/api/transactions/pay` | JWT | Initiate peer payment |
| GET | `/api/transactions/{id}` | JWT | Get single transaction |
| GET | `/api/transactions/history` | JWT | Paginated tx history |
| GET | `/api/transactions/ledger` | JWT | Paginated ledger entries |
| GET | `/api/transactions/ledger/balance` | JWT | Ledger-derived balance |

> ⚠️ All POST `/pay` requests must include `Idempotency-Key` header.

### Migration status
```bash
# Already run — tables exist in ledger_db:
# Transactions, LedgerEntries, IdempotencyKeys, __EFMigrationsHistory
```

---

## 🔧 NuGet Packages — Standard Set Per Service

Every service uses this base set (all .NET 8 compatible):

```xml
Microsoft.EntityFrameworkCore                   8.0.8
Microsoft.EntityFrameworkCore.SqlServer         8.0.8
Microsoft.EntityFrameworkCore.Tools             8.0.8
Microsoft.AspNetCore.Authentication.JwtBearer   8.0.8
Serilog.AspNetCore                              8.0.2
Serilog.Sinks.Console                           6.0.0
Serilog.Sinks.File                              6.0.0
RabbitMQ.Client                                 6.8.1
Swashbuckle.AspNetCore                          6.8.1
```

**Auth.API only:**
```xml
BCrypt.Net-Next                                 4.0.3
FluentValidation.AspNetCore                     11.3.0
AutoMapper.Extensions.Microsoft.DependencyInjection  12.0.1
```

**ApiGateway only:**
```xml
Ocelot                                          23.3.3
Ocelot.Provider.Polly                           23.3.3
```

---

## 🔁 Recurring Patterns (Copy-Paste for New Services)

### Program.cs boilerplate
Every new service's `Program.cs` follows this exact structure:
```
1. Serilog setup
2. AddDbContext with UseSqlServer + retry
3. AddAuthentication JwtBearer (same key/issuer/audience)
4. AddAuthorization
5. AddScoped<IRepository, Repository>
6. AddScoped<IService, Service>
7. AddSingleton<IRabbitMQPublisher>
8. AddSingleton<EventPublisher>
9. AddHostedService<Consumer> (if service has consumers)
10. AddControllers + AddSwaggerGen with Bearer
11. AddHealthChecks + DbContextCheck
12. Middleware: ExceptionMiddleware → Serilog → Auth → Authorization
13. Auto-migrate in Development
```

### ExceptionMiddleware
Identical across all services — only the namespace changes:
```csharp
InvalidOperationException   → 400 Bad Request
UnauthorizedAccessException → 401 Unauthorized
KeyNotFoundException        → 404 Not Found
Everything else             → 500 Internal Server Error
```

### RabbitMQ Consumer boilerplate
```csharp
// Always:
DispatchConsumersAsync = true   // on ConnectionFactory
BasicQos(0, 1, false)           // process one at a time
autoAck: false                  // manual acknowledgement
BasicAck after success
BasicNack with requeue: !args.Redelivered on failure
IServiceProvider.CreateScope()  // for Scoped services inside Singleton consumer
```

---

## 🚀 How to Run the Project

### Prerequisites check
```bash
dotnet --version        # must be 8.0.x
node --version          # must be 20.x
rabbitmq-service start  # RabbitMQ must be running
# SQL Server Express must be running (check Services)
```

### Start order (important — gateway last)
```bash
# Terminal 1
dotnet run --project src\Services\Auth\Auth.API

# Terminal 2
dotnet run --project src\Services\Wallet\Wallet.API

# Terminal 3
dotnet run --project src\Services\Transaction\Transaction.API

# Terminal 4 (after all services are up)
dotnet run --project src\Gateway\ApiGateway
```

Or in Visual Studio: Right-click Solution → **Set Startup Projects** → **Multiple startup projects** → set all four to **Start**.

### Verify everything is running
```
http://localhost:5010/swagger   → Auth API docs
http://localhost:5020/swagger   → Wallet API docs
http://localhost:5030/swagger   → Transaction API docs
http://localhost:5000/health/auth    → "Healthy"
http://localhost:15672          → RabbitMQ Management (guest/guest)
```

---

## 🧪 Quick Smoke Test Sequence

```
1. POST /api/auth/register         → creates user + triggers wallet creation
2. POST /api/auth/login            → get JWT token
3. GET  /api/wallet                → confirm wallet exists (status: Pending)
4. POST /api/kyc/submit            → submit KYC document
5. POST /api/kyc/{id}/approve      → approve as admin → wallet becomes Active
6. POST /api/wallet/fund           → add money (e.g. ₹1000)
7. POST /api/transactions/pay      → pay another user (include Idempotency-Key header)
8. GET  /api/transactions/ledger   → verify DEBIT entry exists
9. POST /api/transactions/pay      → repeat with same Idempotency-Key → same result returned, no double charge
```

---

## ⏳ Phase 7 — Rewards & Loyalty Service (NEXT)

### What needs to be built
- `LoyaltyAccount` entity — one per user, created on `UserRegisteredEvent`
- `RewardTier` — Bronze / Silver / Gold / Platinum thresholds
- `PointRule` — configurable rules (e.g. 1 point per ₹10 spent)
- `PointTransaction` — immutable log of points earned/spent
- Consumer for `TransactionCompletedEvent` → calculate and award points
- Consumer for `RedemptionRequestedEvent` → deduct points
- Publishes `PointsAwardedEvent` → Notification service
- Admin APIs to manage tiers and point rules

### Entities to create
```csharp
LoyaltyAccount  { Id, UserId, TotalPoints, LifetimePoints, TierId, CreatedAt }
RewardTier      { Id, Name, MinPoints, MaxPoints, MultiplierFactor }
PointRule       { Id, TransactionType, PointsPerRupee, IsActive }
PointTransaction { Id, LoyaltyAccountId, Points, Type (Earn/Redeem), ReferenceId, CreatedAt }
```

### Key business logic
```
OnTransactionCompleted:
  1. Find applicable PointRule for TransactionType
  2. Calculate points = Amount * PointsPerRupee * TierMultiplier
  3. Credit points to LoyaltyAccount
  4. Check if new total crosses next tier threshold → upgrade tier
  5. Publish PointsAwardedEvent
```

---

## 📝 Important Decisions Made

| Decision | Choice | Reason |
|----------|--------|--------|
| Database | SQL Server Express (POLLY\SQLEXPRESS) | Already installed, Integrated Security |
| ORM Provider | `Microsoft.EntityFrameworkCore.SqlServer 8.0.8` | Replaced MySQL/Pomelo |
| Auth strategy | JWT + Refresh Token rotation | Stateless, scalable |
| Password hashing | BCrypt (cost factor 11) | Industry standard |
| Event bus | RabbitMQ with manual ack + DLQ | Reliable delivery |
| Ledger design | Immutable, double-entry | Financial audit compliance |
| Idempotency | Client-supplied `Idempotency-Key` header | Prevents duplicate payments |
| Saga pattern | Choreography (no central orchestrator) | Simpler, loosely coupled |
| Enum storage | `HasConversion<string>()` | Human-readable in DB |
| Decimal precision | `decimal(18,2)` explicit | Prevents EF migration drift |
| Error handling | Global ExceptionMiddleware per service | Consistent API responses |
| Response shape | `ApiResponse<T>` wrapper from Shared lib | Consistent across all APIs |

---

## ⚠️ Known Issues / Watch Points

1. **ocelot.json comments** — Remove all `//` comment lines before running. Pure JSON required.
2. **Copy to Output Directory** — `ocelot.json` must be set to `Copy Always` or gateway fails to start.
3. **RabbitMQ must start before services** — Consumers throw `BrokerUnreachableException` if RabbitMQ is down at startup. Start RabbitMQ first.
4. **Migration conflicts** — If you change an entity after running migrations, always `dotnet ef migrations add <name>` before `database update`. Never edit migration files manually.
5. **Wallet namespace collision** — `Wallet.API.Entities.Wallet` conflicts with the project namespace. Always use the full namespace `Wallet.API.Entities.Wallet` in DbContext and Repository files.
6. **Transaction namespace collision** — Same issue: use `Transaction.API.Entities.Transaction` explicitly.

---

## 📁 Shared Library Contents

`WalletPlatform.Shared` — referenced by all services, never references any service.

```
WalletPlatform.Shared/
├── Constants/
│   └── RabbitMQQueues.cs          ← All queue name constants
├── Events/
│   ├── UserRegisteredEvent.cs
│   ├── KYCApprovedEvent.cs
│   ├── WalletFundedEvent.cs
│   ├── PaymentInitiatedEvent.cs
│   ├── TransactionCompletedEvent.cs
│   └── TransactionFailedEvent.cs
├── Messaging/
│   ├── IRabbitMQPublisher.cs
│   └── RabbitMQPublisher.cs       ← Singleton, DLX wired on every queue
└── Models/
    └── ApiResponse<T>.cs          ← Standard response envelope
```

**Events still to add** (for Phases 7–9):
```csharp
PointsAwardedEvent      { UserId, Points, TransactionId, OccurredAt }
RedemptionRequestedEvent { UserId, ItemId, PointsCost, OccurredAt }
```

---

*End of handoff document. Resume at Phase 7: Rewards & Loyalty Service.*
