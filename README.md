# 🏦 WalletPlatform — Microservices Wallet & Payment System

A production-grade **Wallet & Payment Platform** built with **.NET 8 Microservices**, inspired by Paytm/PhonePe. Features a complete event-driven architecture with JWT authentication, double-entry ledger, rewards system, and an Angular frontend.

---

## 📸 Tech Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core Web API (.NET 8) |
| Frontend | Angular 17 |
| API Gateway | Ocelot |
| Message Broker | RabbitMQ |
| Database | SQL Server Express |
| ORM | Entity Framework Core 8 |
| Authentication | JWT + Refresh Tokens |
| Logging | Serilog |
| Documentation | Swagger / OpenAPI |

---

## 🏗️ Architecture

```
Angular Client (:4200)
        │
        ▼
API Gateway — Ocelot (:5000)
JWT Validation · Rate Limiting · Circuit Breaker (Polly)
        │
        ├──► Auth.API          :5010  →  auth_db
        ├──► Wallet.API        :5020  →  wallet_db
        ├──► Transaction.API   :5030  →  ledger_db
        ├──► Rewards.API       :5040  →  rewards_db
        ├──► Catalog.API       :5050  →  catalog_db
        ├──► Notification.API  :5080  →  notification_db
        └──► Reporting.API     :5070  →  reporting_db
                │
                ▼
           RabbitMQ (:5672)
     Event-driven async messaging
```

---

## 📦 Microservices

| Service | Port | Description |
|---|---|---|
| **Auth.API** | 5010 | User registration, login, JWT, KYC workflow |
| **Wallet.API** | 5020 | Wallet management, balance, freeze/unfreeze |
| **Transaction.API** | 5030 | Double-entry ledger, idempotency, Saga pattern |
| **Rewards.API** | 5040 | Points engine, tier upgrades, loyalty accounts |
| **Catalog.API** | 5050 | Reward catalog, point redemptions |
| **Notification.API** | 5080 | Email/SMS simulation, notification logs |
| **Reporting.API** | 5070 | Report definitions (extensible) |
| **ApiGateway** | 5000 | Ocelot routing, JWT validation, rate limiting |

---

## ✨ Features

### Security
- JWT access tokens + refresh token rotation
- Role-based authorization (User / Admin)
- Rate limiting per route via Ocelot
- Audit logging on all auth actions
- Wallet freeze / unfreeze by admin

### Payments
- Peer-to-peer wallet transfers
- Double-entry accounting (immutable ledger)
- Idempotency keys — prevents duplicate payments
- Saga compensation pattern — auto-reversal on failure
- Balance snapshots on every ledger entry

### Rewards
- Configurable point rules per transaction type
- Tier system: Bronze → Silver → Gold → Platinum
- Tier multipliers (1x, 1.5x, 2x, 3x)
- Point redemption via catalog items
- Voucher code generation on redemption

### Event-Driven Architecture
- RabbitMQ async messaging between services
- Dead-letter queues for failed messages
- Manual acknowledgement — no message loss
- Each service has its own queue per event type

### KYC Workflow
- Document submission
- Admin review queue
- Approve / Reject with reason
- Wallet auto-activates on KYC approval

---

## 🚀 Getting Started

### Prerequisites

```
- .NET 8 SDK
- SQL Server Express
- RabbitMQ (with Erlang)
- Node.js 20+
- Angular CLI 17
```

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/WalletPlatform.git
cd WalletPlatform
```

### 2. Set Up Databases

Open SSMS and run:

```sql
CREATE DATABASE auth_db;
CREATE DATABASE wallet_db;
CREATE DATABASE ledger_db;
CREATE DATABASE rewards_db;
CREATE DATABASE catalog_db;
CREATE DATABASE notification_db;
CREATE DATABASE reporting_db;
```

### 3. Update Connection Strings

Each service has its own `appsettings.json`. Update the connection string to match your SQL Server instance:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=YOUR_SERVER\\SQLEXPRESS;Initial Catalog=auth_db;Integrated Security=True;TrustServerCertificate=True;"
}
```

### 4. Run Migrations

```powershell
dotnet ef database update --project src\Services\Auth\Auth.API
dotnet ef database update --project src\Services\Wallet\Wallet.API
dotnet ef database update --project src\Services\Transaction\Transaction.API
dotnet ef database update --project src\Services\Rewards\Rewards.API
dotnet ef database update --project src\Services\Catalog\Catalog.API
dotnet ef database update --project src\Services\Notification\Notification.API
```

### 5. Install Frontend Dependencies

```bash
cd frontend/wallet-app
npm install
```

### 6. Build Solution

```bash
cd WalletPlatform
dotnet build WalletPlatform.sln
```

### 7. Start All Services

Run the startup script:

```powershell
.\start.ps1
```

Or manually start each service:

```powershell
# Open separate terminal for each
dotnet run --no-build --project src\Services\Auth\Auth.API
dotnet run --no-build --project src\Services\Wallet\Wallet.API
dotnet run --no-build --project src\Services\Transaction\Transaction.API
dotnet run --no-build --project src\Services\Rewards\Rewards.API
dotnet run --no-build --project src\Services\Catalog\Catalog.API
dotnet run --no-build --project src\Services\Notification\Notification.API
dotnet run --no-build --project src\Gateway\ApiGateway

# Frontend
cd frontend/wallet-app
ng serve --port 4200
```

### 8. Access the Application

| URL | Description |
|---|---|
| http://localhost:4200 | Angular Frontend |
| http://localhost:5000 | API Gateway |
| http://localhost:5010/swagger | Auth API Docs |
| http://localhost:5020/swagger | Wallet API Docs |
| http://localhost:5030/swagger | Transaction API Docs |
| http://localhost:5040/swagger | Rewards API Docs |
| http://localhost:5050/swagger | Catalog API Docs |
| http://localhost:5080/swagger | Notification API Docs |
| http://localhost:15672 | RabbitMQ Dashboard (guest/guest) |

---

## 📁 Project Structure

```
WalletPlatform/
├── src/
│   ├── Gateway/
│   │   └── ApiGateway/              ← Ocelot gateway
│   ├── Services/
│   │   ├── Auth/Auth.API/
│   │   ├── Wallet/Wallet.API/
│   │   ├── Transaction/Transaction.API/
│   │   ├── Rewards/Rewards.API/
│   │   ├── Catalog/Catalog.API/
│   │   ├── Notification/Notification.API/
│   │   └── Reporting/Reporting.API/
│   └── Shared/
│       └── WalletPlatform.Shared/   ← Shared events & contracts
├── frontend/
│   └── wallet-app/                  ← Angular 17 app
├── start.ps1                        ← Start all services
├── stop.ps1                         ← Stop all services
├── .gitignore
└── WalletPlatform.sln
```

---

## 🔄 Event Flow

```
User Registers
    └──► UserRegisteredEvent
             ├──► Wallet.API     → Creates wallet (Pending)
             ├──► Rewards.API    → Creates loyalty account
             └──► Notification   → Sends welcome email

KYC Approved
    └──► KYCApprovedEvent
             ├──► Wallet.API     → Activates wallet
             └──► Notification   → Sends KYC approval email

Payment Made
    └──► TransactionCompletedEvent
             ├──► Rewards.API    → Awards points, checks tier upgrade
             └──► Notification   → Sends payment confirmation

Points Redeemed
    └──► RedemptionRequestedEvent
             └──► Rewards.API    → Deducts points from account
```

---

## 🔑 Default JWT Configuration

```json
{
  "Jwt": {
    "Key": "WalletPlatform_SuperSecret_JWT_Key_2024_MustBe32CharsMin!!",
    "Issuer": "WalletPlatform.Auth",
    "Audience": "WalletPlatform.Client",
    "ExpiryMinutes": "60"
  }
}
```

> ⚠️ Change the JWT key before deploying to production.

---

## 🧪 Quick API Test

### Register
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Rahul","lastName":"Sharma","email":"rahul@test.com","password":"Test@1234","phoneNumber":"+919876543210"}'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"rahul@test.com","password":"Test@1234"}'
```

### Browse Catalog (no auth)
```bash
curl http://localhost:5000/api/catalog
```

### Get Wallet (with token)
```bash
curl http://localhost:5000/api/wallet \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

---

## 🏦 Reward Tiers

| Tier | Points Required | Multiplier |
|---|---|---|
| 🥉 Bronze | 0 - 999 | 1.0x |
| 🥈 Silver | 1,000 - 4,999 | 1.5x |
| 🥇 Gold | 5,000 - 19,999 | 2.0x |
| 💎 Platinum | 20,000+ | 3.0x |

---

## 📝 Point Rules

| Transaction Type | Points per ₹ | Max per Transaction |
|---|---|---|
| Peer Transfer | 0.1 (1 pt per ₹10) | 500 pts |
| Merchant Pay | 0.2 (1 pt per ₹5) | 1,000 pts |

---

## 🔒 Security Features

- Passwords hashed with BCrypt (cost factor 11)
- JWT tokens expire in 60 minutes
- Refresh tokens expire in 7 days with rotation
- Rate limiting: 5 register/min, 10 login/min
- Circuit breaker: opens after 3 failures, resets after 10s
- No raw card numbers stored — tokenization concept applied
- Audit log on every auth action

---

## 🗄️ Database Design

Each service owns its database — **no cross-service DB joins ever**.

| Database | Tables |
|---|---|
| auth_db | Users, UserProfiles, KYCRecords, RefreshTokens, AuditLogs |
| wallet_db | Wallets, PaymentMethods, WalletFreezeLogs |
| ledger_db | Transactions, LedgerEntries, IdempotencyKeys |
| rewards_db | LoyaltyAccounts, RewardTiers, PointRules, PointTransactions |
| catalog_db | CatalogItems, Redemptions |
| notification_db | NotificationLogs, NotificationTemplates |
| reporting_db | ReportDefinitions |

---

## 🚧 Roadmap

- [ ] Docker Compose for one-command startup
- [ ] Real email provider (SendGrid integration)
- [ ] Real SMS provider (Twilio integration)
- [ ] Admin dashboard UI
- [ ] Merchant payment flow
- [ ] Card tokenization (Stripe integration)
- [ ] Transaction export to PDF/Excel
- [ ] Push notifications
- [ ] Two-factor authentication (2FA)
- [ ] API versioning

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

---

