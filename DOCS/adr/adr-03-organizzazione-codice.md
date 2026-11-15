# ADR-03 — Organizzazione del codice e convenzioni

- **Stato:** Accettato
- **Data:** 26/10/2026
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Tre persone lavorano su tre aree dello stesso repo. Dove va ogni pezzo di codice, chi lo possiede e quali convenzioni seguiamo, per evitare conflitti e incomprensioni?

## Opzioni considerate

- proprietà delle cartelle
- porte di sviluppo
- lingua

## Decisioni

| Domanda | Decisione |
|---|---|
| Codice del db | Proprietà e attività di Elmond0 |
| Codice del client | Proprietà e attività di fmartini02 |
| Porte in sviluppo | API port: 5290, client port: 5100 |
| Lingua delle variabili | inglese |
| Lingua degli indirizzi | inglese |
| Lingua dei DOCS | italiano/eventualmente un README in inglese |

### Conseguenze

Territori aggiornati in [git-workflow.md](../git-workflow.md)

## Fonti

- [IDE0130: namespace e cartelle](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0130) · [CORS](https://learn.microsoft.com/aspnet/core/security/cors) · [Regole per i nomi in C#](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/identifier-names)
