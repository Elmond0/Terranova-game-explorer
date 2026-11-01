# ADR-06 — Accordi della fase 2: account, preferiti e wishlist

- **Stato:** Proposto <!-- Proposto → Accettato, quando tutti i punti della #55 sono decisi -->
- **Data:** 01/11/2026 (punto 10)
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Le decisioni della fase 2, prese nella #55: una riga per punto nella tabella "Decisione". I punti 1–9 si aggiungono quando sono decisi.

**Punto 10.** L'ADR-05 aveva scelto `game_cache`, una cache su database letta e scritta da `RawgService`. Serve davvero alla scheda gioco, o basta la cache in memoria? E da dove prendono nome e copertina preferiti, wishlist e tier list?

## Opzioni discusse (punto 10)

- **A.** `game_cache` come nell'ADR-05: memoria → database se fresco → RAWG → copia scaduta se RAWG è giù.
- **B.** La scheda usa solo la cache in memoria; la tabella diventa la copia dei giochi salvati dagli utenti, scritta solo quando un utente salva.
- **C.** Nessuna tabella: le liste chiedono ogni gioco a RAWG.

## Decisione

| Punto | Decisione |
|---|---|
| 10. Copia dei giochi salvati | **B.** La scheda usa solo `HybridCache` e RAWG. `game_cache` diventa **`game_snapshots`**: una riga per ogni gioco salvato da almeno un utente, scritta solo dal salvataggio di preferiti, wishlist e tier list (che rifà la foto), letta solo dalle liste dell'utente. Nessuna scadenza; lo slug non è più unico. **Sostituisce il punto "Cache dei giochi" dell'ADR-05.** A è scartata perché la quota RAWG basta con la sola memoria e il percorso a quattro livelli pesava sul backend; C perché le liste sarebbero lente, consumerebbero quota e sarebbero vuote con RAWG giù |

### Conseguenze

- **Elmond0:** la #35, riscritta, rinomina la tabella con una migrazione `RenameTable` (prima la tabella, poi le classi, altrimenti EF la ricrea); toglie `IGameCacheStore`, `FindAsync` e `GameCachePolicy`; aggiorna `database.yml`.
- **K0ff33Blu3:** `RawgService` non cambia. Rivede la PR della #35, che toglie il finto `EmptyGameCacheStore` da `ApiFactory.cs`.
- **fmartini02:** niente. Il contratto delle API non cambia.
- Con RAWG giù la scheda risponde `502`, se il gioco non è già in memoria. La card di un preferito può mostrare dati vecchi finché qualcuno non risalva il gioco; la scheda è sempre aggiornata.

## Fonti

- [HybridCache in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid) · [EF Core: gestire le migrazioni](https://learn.microsoft.com/ef/core/managing-schemas/migrations/managing) · [PostgreSQL: ALTER TABLE ... RENAME](https://www.postgresql.org/docs/current/sql-altertable.html)
