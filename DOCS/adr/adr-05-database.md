# ADR-05 — Database: utenti, migrazioni e cosa salviamo

- **Stato:** Accettato
- **Data:** 26/10/2026. Aggiornato l'08/11/2026 (passaggio a IGDB) e il 12/11/2026 (tolta la copia dei giochi, vedi ADR-06)
- **Decisori:** Elmond0 (database), con la revisione di K0ff33Blu3 e fmartini02

## Contesto e problema

Lo schema del database lo gestisce EF Core, dentro la Web API. Dovevamo decidere chi può modificare le tabelle, come si applicano le migrazioni, che cosa teniamo nel database dei giochi che arrivano da IGDB e con che nomi.

## Opzioni considerate

| Domanda | Opzioni |
|---|---|
| Utenti PostgreSQL | **A.** due utenti: uno proprietario per le migrazioni (`terranova_app`), uno che può solo leggere e scrivere righe per la Web API (`terranova_api`) · **B.** un utente solo per tutto |
| Applicare le migrazioni | **A.** script SQL idempotente applicato da `db/scripts/db.cs migrate` · **B.** `dotnet ef database update` · **C.** `Database.Migrate()` all'avvio della Web API |
| Giochi nel database | **A.** solo gli id dei giochi salvati dagli utenti · **B.** una copia dei giochi salvati (`game_snapshots`) · **C.** una cache su database di tutti i giochi visti (`game_cache`) |
| Nomi | **A.** inglese, `snake_case` · **B.** italiano |

## Decisione

**Due utenti (A).** `terranova_app` è il proprietario del database e lo usiamo solo per migrazioni e restore. La Web API si collega come `terranova_api`, che legge e scrive righe ma non può creare, cambiare o cancellare tabelle. Se qualcuno trovasse un modo di far eseguire SQL alla Web API, non potrebbe comunque toccare lo schema. Gli utenti li creano gli script in `db/init/` al primo avvio del container, con le password lette dai file in `secrets/`.

Il registro delle migrazioni (`__EFMigrationsHistory`) la Web API lo può leggere ma non modificare: lo script aggiunge un `REVOKE` in fondo a ogni applicazione.

**Migrazioni con uno script (A).** `db.cs migrate` genera l'SQL con `dotnet ef migrations script --idempotent` e lo applica come `terranova_app` dentro il container. L'SQL si può leggere prima di applicarlo e lo script si può rilanciare quante volte si vuole: le migrazioni già registrate vengono saltate. `database update` (B) avrebbe bisogno della password del proprietario nella configurazione della Web API; `Database.Migrate()` (C) farebbe girare la Web API come proprietario, il contrario di quello che vogliamo.

**Solo gli id dei giochi (A).** Il database tiene utenti, preferiti e wishlist; di un gioco salva solo l'id IGDB e la data in cui è stato aggiunto. Nome, copertina e il resto arrivano da IGDB quando servono e restano nella cache in memoria della Web API. Ci siamo arrivati in due passi: prima avevamo una cache su database di tutti i giochi (C), poi una copia dei soli giochi salvati (B). Le ragioni per cui le abbiamo tolte sono nell'ADR-06.

**Inglese e `snake_case` (A).** È la convenzione di PostgreSQL ed evita di dover mettere tra virgolette ogni nome. La attiva `UseSnakeCaseNamingConvention()` in `Program.cs`, così nel codice C# i nomi restano in `PascalCase`.

### Lo schema oggi

| Tabella | Contenuto |
|---|---|
| `users` | email, hash della password, timbro di sessione, nickname, avatar, data di creazione |
| `favorites` | `user_id`, `game_id`, `added_at`; chiave (`user_id`, `game_id`) |
| `wishlist` | come `favorites` |
| `__EFMigrationsHistory` | le migrazioni applicate (di EF Core) |

`email` e `display_name` sono unici senza distinguere maiuscole e minuscole: usano una collation ICU non deterministica (`und-u-ks-level2`), così `Mario@x.it` e `mario@x.it` sono la stessa email anche per l'indice unico.

### Conseguenze

- **Chi crea le migrazioni:** chi modifica le entità in `be/GameExplorer.Api/Data/` aggiunge la migrazione nello stesso commit. La CI (`.github/workflows/database.yml`) fallisce se ne manca una (`dotnet ef migrations has-pending-model-changes`) e controlla che si possano applicare due volte di fila.
- **Migrazioni scritte a mano:** quando EF non sa fare da solo una trasformazione (per esempio cambiare il tipo di `user_id` da testo a numero) correggiamo il file generato e leggiamo l'SQL prima di applicarlo. Gli appunti sono in `DOCS/study-notes.md`.
- **Dati di prova:** alcune migrazioni hanno cancellato i dati esistenti (il passaggio da RAWG a IGDB cambia gli id dei giochi, quello da Identity a `users` cambia gli utenti). Prima di applicarle si fa un backup con `dotnet db/scripts/db.cs backup`.
- **Solo in locale:** lo script usa `docker compose exec`, quindi funziona solo con il database del compose.

**Aggiornamento (novembre 2026):** `migrate.sh` e `backup.sh` sono diventati un solo file C# eseguito con `dotnet` (`db/scripts/db.cs`), perché su Windows gli script bash richiedevano Git Bash. Il metodo scelto (A) non cambia.

## Fonti

- [Applicare le migrazioni](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying) · [ALTER DEFAULT PRIVILEGES](https://www.postgresql.org/docs/current/sql-alterdefaultprivileges.html) · [Collation non deterministiche in PostgreSQL](https://www.postgresql.org/docs/current/collation.html#COLLATION-NONDETERMINISTIC) · [Npgsql: collation e confronti senza maiuscole](https://www.npgsql.org/efcore/misc/collations-and-case-sensitivity.html) · [`db/README.md`](../../db/README.md)
