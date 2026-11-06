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

> **Motivazione:** Al posto di Angular abbiamo scelto Blazor WebAssembly per usare un solo linguaggio, C#, in frontend e backend: Angular (e React, che avevamo considerato) richiede TypeScript e ci sembrava più complesso da imparare. Così i modelli di `shared/` sono usati sia dalla Web API sia dal client. Blazor offre comunque strumenti simili ad Angular: template di progetto pronti, componenti riutilizzabili, routing e dependency injection. Sappiamo che il primo caricamento è più lento (poi il runtime .NET resta in cache quindi, la navigazione rimane veloce) e che la community è più piccola, ma era la scelta che richiedeva meno sforzo per imparare. Al posto di Bootstrap usiamo MudBlazor: componenti Material già pronti, solo in C#, con il tema chiaro/scuro incluso.

## 3. Database

**PostgreSQL**, con **Entity Framework Core** come ORM e **Npgsql** come provider.

> **Motivazione:** Al posto di SQL Server Express usiamo PostgreSQL perché è open source. Inoltre lo avevo già in parte studiato per un altro progetto (Transcendence), quindi partiamo con meno tempo da dedicare a imparare il database.
