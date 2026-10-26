# ADR-03 — Organizzazione del codice e convenzioni

- **Stato:** Accettato
- **Data:** 26/10/2026
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Tre persone lavorano su tre aree dello stesso repo. Dove va ogni pezzo di codice, chi lo possiede e quali convenzioni seguiamo, per evitare conflitti e incomprensioni?

## Opzioni considerate

- proprieta delle cartelle
- porte di sviluppo
- lingua

## Decisioni

| Domanda | Proposta |
|---|---|
| Codice del db | Proprieta della cartella di K0ff33Blu3, ci lavora elmondo |
| Codice del client | Proprieta e attivita di di fmartini02 |
| Porte in sviluppo | API port: 5290, client port: 5100 |
| Lingua delle variabili | inglese |
| Lingua degli indirizzi | inglese |
| Lingua dei DOCS | italiano/eventualmente un README in inglese |

### Conseguenze

Territori aggiornati in come_lavoriamo.md

## Fonti

- [Migrazioni in un progetto separato](https://learn.microsoft.com/ef/core/managing-schemas/migrations/projects) · [IDE0130: namespace e cartelle](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0130) · [CORS](https://learn.microsoft.com/aspnet/core/security/cors) · [Regole per i nomi in C#](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names)
