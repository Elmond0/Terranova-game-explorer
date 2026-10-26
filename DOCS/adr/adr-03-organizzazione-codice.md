# ADR-03 — Organizzazione del codice e convenzioni

- **Stato:** Proposto <!-- Proposto → Accettato -->
- **Data:** <!-- gg/mm/aaaa della decisione -->
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Tre persone lavorano su tre aree dello stesso repo. Dove va ogni pezzo di codice, chi lo possiede e quali convenzioni seguiamo, per evitare conflitti e incomprensioni?

## Opzioni considerate

| Domanda | Opzioni |
|---|---|
| Codice del database | **A.** cartella `be/GameExplorer.Api/Data/`, di C · **B.** progetto separato `GameExplorer.Data` |
| Chi crea `fe/GameExplorer.Client` | **A.** B (la cartella è sua) · **B.** A (come dice la `Suddivisione`) |
| Cartella `shared/.../models/` | **A.** rinominarla `Models/`, come il namespace · **B.** lasciarla |
| Porte in sviluppo | **A.** fisse: API `5290`, client `5100` · **B.** quelle del template |
| Indirizzi delle pagine | **A.** italiano (`/ricerca`, `/gioco/{id}`, `/studio/{id}`) · **B.** inglese (`/search`, `/games/{id}`, `/studios/{id}`) |
| Tema chiaro/scuro salvato | **A.** lo fa B · **B.** lo fa A |
| Chi rivede le pagine di C in `fe/` | **A.** B · **B.** chiunque |
| Lingua dei nomi nel codice | **A.** inglese (testi per l'utente in italiano) · **B.** italiano |

## Decisione

<!-- Per ogni domanda: opzione scelta e perché, in una riga. -->

### Conseguenze

<!-- Territori da aggiornare in DOCS/come_lavoriamo.md, file da rinominare, impostazioni da cambiare. -->

## Fonti

- [Migrazioni in un progetto separato](https://learn.microsoft.com/ef/core/managing-schemas/migrations/projects) · [IDE0130: namespace e cartelle](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0130) · [CORS](https://learn.microsoft.com/aspnet/core/security/cors) · [Regole per i nomi in C#](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names)
