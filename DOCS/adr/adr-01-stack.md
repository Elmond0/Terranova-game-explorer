# ADR-01 — Stack tecnologico

| Stato | Data |
|---|---|
| ✅ Accettato | 20/10/2026 |

## Riepilogo

| Livello | Tecnologia |
|---|---|
| Backend | C# · ASP.NET Core Web API (.NET 10) |
| Frontend | C# · Blazor WebAssembly · MudBlazor |
| Database | PostgreSQL · Entity Framework Core · Npgsql |

---

## 1. Backend

**C# con ASP.NET Core Web API (.NET 10)**

> **Motivazione:**
Stack consigliato da Terranova, inoltre C# è uno dei linguaggi più veloci nella gestione di chiamate API REST. ASP.NET framework .NET per web app.

## 2. Frontend

**Blazor WebAssembly (C#) con MudBlazor** come libreria di componenti

> **Motivazione:** _da completare_

## 3. Database

**PostgreSQL**, con **Entity Framework Core** come ORM e **Npgsql** come provider.

> **Motivazione:** Al posto di SQL Server Express usiamo PostgreSQL perché è open source. Inoltre lo avevo già in parte studiato per un altro progetto (Transcendence), quindi partiamo con meno tempo da dedicare a imparare il database.
