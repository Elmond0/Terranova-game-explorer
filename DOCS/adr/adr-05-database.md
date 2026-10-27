# ADR-05 — Database: utenti, migrazioni e cache

- **Stato:** Accettato <!-- Proposto → Accettato -->
- **Data:** <!-- gg/mm/aaaa della decisione -->
- **Decisori:** Elmond0 (database), con la revisione di K0ff33Blu3 e fmartini02

## Contesto e problema

Lo schema del database lo gestisce EF Core, dentro la Web API. Chi può modificare le tabelle, come si applicano le migrazioni e come la Web API usa la cache dei giochi presi da RAWG (20.000 richieste al mese)?

## Opzioni considerate

| Domanda | Opzioni |
|---|---|
| Utenti PostgreSQL | **A.** separati: proprietario per le migrazioni (`terranova_app`), solo righe per la Web API (`terranova_api`) · **B.** uno solo per tutto |
| Applicare le migrazioni | **A.** script SQL idempotente con `db/scripts/migrate.sh` · **B.** `dotnet ef database update` · **C.** `Database.Migrate()` all'avvio |
| Cache dei giochi | **A.** tabella `game_cache` + interfaccia `IGameCacheStore` usata da `RawgService` · **B.** solo cache in memoria · **C.** Redis |
| Nomi nel database | **A.** inglese, `snake_case` (`game_cache`) · **B.** italiano |

## Decisione

- **Utenti PostgreSQL:** A. `terranova_app` (proprietario) solo per migrazioni e restore; la Web API si collega come `terranova_api`, che legge e scrive righe ma non può cambiare le tabelle. Privilegio minimo.
- **Migrazioni:** A. `dotnet ef migrations script --idempotent` + `db/scripts/migrate.sh` come `terranova_app`. L'SQL si legge prima di applicarlo, si può rilanciare, funziona uguale in locale e su Neon. Niente `database update` né `Database.Migrate()`.
- **Cache dei giochi:** A. Tabella `game_cache` letta e scritta da `RawgService` tramite `IGameCacheStore` (`FindAsync`, `SaveAsync`). Sopravvive ai riavvii e servirà a preferiti e wishlist; la cache in memoria resta per le risposte.
- **Nomi nel database:** A. Inglese, `snake_case`, come vuole PostgreSQL.
### Conseguenze

<!-- Chi crea le migrazioni, cosa serve su Neon, cosa controlla la CI. -->

## Fonti

- [Applicare le migrazioni](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying) · [ALTER DEFAULT PRIVILEGES](https://www.postgresql.org/docs/current/sql-alterdefaultprivileges.html) · [`db/README.md`](../../db/README.md)
