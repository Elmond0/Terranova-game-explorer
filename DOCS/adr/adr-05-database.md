# ADR-05 — Database: utenti, migrazioni e copia dei giochi

- **Stato:** Accettato <!-- Proposto → Accettato -->
- **Data:** 26/10/2026 (aggiornato il 08/11/2026: tolta la cache dei giochi su database, passaggio da RAWG a IGDB)
- **Decisori:** Elmond0 (database), con la revisione di K0ff33Blu3 e fmartini02

## Contesto e problema

Lo schema del database lo gestisce EF Core, dentro la Web API. Chi può modificare le tabelle, come si applicano le migrazioni e che cosa salva il database dei giochi che arrivano da IGDB?

## Opzioni considerate

| Domanda | Opzioni |
|---|---|
| Utenti PostgreSQL | **A.** separati: proprietario per le migrazioni (`terranova_app`), solo righe per la Web API (`terranova_api`) · **B.** uno solo per tutto |
| Applicare le migrazioni | **A.** script SQL idempotente con `db/scripts/migrate.sh` · **B.** `dotnet ef database update` · **C.** `Database.Migrate()` all'avvio |
| Giochi nel database | **A.** nessuna cache su database: la scheda usa solo la cache in memoria (`HybridCache`) e la tabella `game_snapshots` tiene la copia dei giochi salvati dagli utenti ([ADR-06](adr-06-account-preferiti.md), punto 10) · **B.** una cache di tutti i giochi visti su database (la vecchia `game_cache`) · **C.** Redis |
| Nomi nel database | **A.** inglese, `snake_case` (`game_snapshots`) · **B.** italiano |

## Decisione

- **Utenti PostgreSQL:** A. `terranova_app` (proprietario del database) solo per migrazioni e restore; la Web API si collega come `terranova_api`, che legge e scrive righe ma non può cambiare le tabelle. Privilegio minimo. Gli utenti li crea `db/init/` al primo avvio del container, con le password lette da `secrets/`.
- **Migrazioni:** A. `dotnet ef migrations script --idempotent` + `db/scripts/migrate.sh`, che applica l'SQL come `terranova_app` dentro il container. L'SQL si legge prima di applicarlo e si può rilanciare (le migrazioni già in `__EFMigrationsHistory` sono saltate). Niente `database update` né `Database.Migrate()`.
- **Giochi nel database:** A. Nessuna cache dei giochi su database: la scheda di un gioco usa solo la cache in memoria e IGDB. La tabella `game_snapshots` (una riga per gioco, chiave `game_id`) contiene solo i giochi salvati da almeno un utente in preferiti, wishlist o tier list; serve alle liste dell'utente. Questa scelta sostituisce la `game_cache` con `IGameCacheStore` decisa all'inizio: la sostituisce l'[ADR-06](adr-06-account-preferiti.md) (punto 10).
- **Nomi nel database:** A. Inglese, `snake_case`, come vuole PostgreSQL (la convenzione è attiva in `Program.cs`).

### Conseguenze

- **Chi crea le migrazioni:** chi modifica le entità in `be/GameExplorer.Api/Data/` aggiunge anche la migrazione nello stesso commit. La CI (`.github/workflows/database.yml`) fallisce con `dotnet ef migrations has-pending-model-changes` se manca, e controlla che le migrazioni si applichino due volte di fila.
- **Colonne dei giochi:** `rating` è su scala 0-100 come lo restituisce IGDB e `metacritic` non esiste più; l'identificatore è `game_id` (non più `rawg_id`). Lo stato attuale dello schema è nelle migrazioni, non in questo ADR.
- **Neon:** `migrate.sh` oggi usa `docker compose exec` e quindi funziona solo sul database locale. Per Neon serve un'altra via per applicare lo stesso SQL come proprietario: non ancora decisa (si decide con la pubblicazione online).
- **Dati vecchi:** una riga di `game_snapshots` non scade; se IGDB cambia i dati di un gioco salvato, la card nelle liste resta com'era finché il gioco non viene salvato di nuovo.

## Fonti

- [Applicare le migrazioni](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying) · [ALTER DEFAULT PRIVILEGES](https://www.postgresql.org/docs/current/sql-alterdefaultprivileges.html) · [`db/README.md`](../../db/README.md) · [IGDB API](https://api-docs.igdb.com/)
