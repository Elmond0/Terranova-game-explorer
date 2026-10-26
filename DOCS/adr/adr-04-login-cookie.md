# ADR-04 — Login: cookie o token

- **Stato:** Proposto <!-- Proposto → Accettato -->
- **Data:** <!-- gg/mm/aaaa della decisione -->
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Preferiti, wishlist e tier list appartengono a un utente. Con ASP.NET Core Identity (`MapIdentityApi`), come dimostra il client Blazor WebAssembly alla Web API chi è l'utente collegato?

## Opzioni considerate

- **A. Cookie `HttpOnly`**: il browser lo conserva e lo rimanda da solo; JavaScript non può leggerlo. Servono CORS con `AllowCredentials` (Web API) e un `CookieHandler` (client).
- **B. Token**: il client lo salva (di solito nel `localStorage`, leggibile da JavaScript) e lo aggiunge a ogni richiesta. Adatto ai client che non sono un browser.

## Decisione

<!-- Opzione scelta e perché, in una o due righe. -->

### Conseguenze

<!-- Cosa devono fare A, B e C; cosa cambia per Postman e per la pubblicazione. -->

## Fonti

- [Identity per le API e le SPA](https://learn.microsoft.com/aspnet/core/security/authentication/identity-api-authorization) · [Blazor WebAssembly con Identity](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity/)
