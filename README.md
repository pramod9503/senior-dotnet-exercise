# Senior Dotnet Exercise

This project is an ASP.NET Core Web API using EF Core and PostgreSQL. It demonstrates how to allocate payments across invoice line items, record all money movement in an append-only ledger, and derive the outstanding invoice balance from the ledger. The implementation is based on a typical real-world scenario for care billing.

---

## The Task

**Implement a method called `AllocatePayment`.**

### Situation

- An invoice has several outstanding line items (amounts owed).
- A payment comes in.
- The method spreads that payment across the line items (oldest first), records what happened in an append-only ledger, and returns the invoice's new outstanding balance.

---

## Data Model

The schema is designed as follows:

- **Invoice** — one bill for a resident's care.
  - `Id` (Guid)
  - `Reference` (e.g. "INV-001")
  - `CreatedAt` (DateTime)

- **InvoiceLineItem** — one charge on that invoice.
  - `Id` (Guid)
  - `InvoiceId` (Guid)
  - `Description` (string)
  - `DueDate` (DateTime)
  - `Amount` (decimal)
  - `CreatedAt` (DateTime)

- **LedgerEntry** — append-only record of money movement. **Never updated or deleted — only inserted.**
  - `Id` (Guid)
  - `InvoiceId` (Guid)
  - `LineItemId` (Guid, nullable)
  - `Type` (`PaymentReceived`, `Allocation`, or `Credit`)
  - `Amount` (decimal)
  - `CreatedAt` (DateTime)

> The outstanding balance is **derived** from the ledger (total charged minus total allocated), not stored in a column.

---

## Method Signature
```C#
// Allocates a payment across an invoice's outstanding line items, oldest first, 
// records the result in the append-only ledger, and returns the invoice's new outstanding balance. A negative result means the invoice is in credit.

Task<decimal> AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt);
```
---

## Allocation Rules

- **Stack:** ASP.NET Core + EF Core + PostgreSQL.
- **Money:** GBP, tracked exactly to the penny (`decimal`).
- **Allocation order:** Oldest line first (earliest `DueDate` first; if tied, earliest `CreatedAt`).
- **Fill each line fully before moving on.**
- **Three payment cases:** partial, exact, overpayment.
- **Append-only ledger:** Only insert new entries; never update or delete.

---

## Worked Examples

All examples start from the same invoice:

| Line | Description        | Due date    | Amount owed |
|------|--------------------|-------------|-------------|
| A    | Care fees — week 1 | 07 Jan 2026 | £100.00     |
| B    | Care fees — week 2 | 14 Jan 2026 | £150.50     |
| C    | Care fees — week 3 | 21 Jan 2026 | £200.25     |

**Total outstanding before any payment: £450.75**

### Example 1 — Partial payment of £120.00

- Line A: £100.00 paid in full. £20.00 left.
- Line B: £20.00 paid. £130.50 left.
- Line C: untouched.

**New outstanding balance returned: £330.75**

Ledger entries: `PaymentReceived £120.00`, `Allocation £100.00 → A`, `Allocation £20.00 → B`.

### Example 2 — Exact payment of £450.75

- All lines cleared.

**New outstanding balance returned: £0.00**

### Example 3 — Overpayment of £500.00

- All lines cleared (£450.75 allocated).
- £49.25 left over → record as a `Credit` (LineItemId null).

**New outstanding balance returned: −£49.25**

---

## What "Done" Looks Like

- [x] `AllocatePayment` implemented over EF Core + PostgreSQL.
- [x] Allocation is oldest-first and fills each line fully before the next.
- [x] Partial, exact, and overpayment cases all behave as in the examples.
- [x] Ledger entries are inserted, never updated or deleted; the balance is derived from them.
- [x] Money is exact to the penny.
- [x] Tests cover the three cases above (xUnit)using in-memory/SQLite for tests

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or higher

### Setup

1. **Clone the repository:**
```
git clone https://github.com/pramod9503/SeniorDotnetExercise.git

cd SeniorDotnetExercise
```

2. **Configure the database connection:**

   - Update `appsettings.json`:

   - "ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=senior_dotnet_exercise;Username=youruser;Password=yourpassword"
    }


- You can use Docker to run PostgreSQL locally:
```
    > docker run --name postgres-dotnet -e POSTGRES_PASSWORD=yourpassword -p 5432:5432 -d postgres
```
    
3. **Apply EF Core migrations:**
```
dotnet ef database update
```

4. **Run the application:**
dotnet run

    The API will be available at `https://localhost:7000`.

5. **Explore the API:**

    Swagger UI is enabled in development mode at `https://localhost:7000/swagger/index.html`.

---

## Testing

- Tests are provided for all three payment scenarios (partial, exact, overpayment).
- xUnit is used.
- Tests use EF Core's in-memory provider (or SQLite in-memory).

---

## Project Structure

- `Models/` — Entity models for EF Core
- `Services/` — Business logic and service implementations
- `Abstracts/` — Service interfaces and abstract classes
- `SeedInvoices.cs` — Seeds the database with initial data if empty
- `Program.cs` — Application entry point and configuration

---
