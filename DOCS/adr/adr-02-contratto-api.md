# ADR-02 — Contratto delle API

- **Stato:** Proposto <!-- Proposto → Accettato -->
- **Data:** <!-- gg/mm/aaaa della decisione -->
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Client e Web API li scrivono persone diverse. Come sono fatti indirizzi, parametri, risposte ed errori della Web API, così che il client possa lavorare con dati finti finché la Web API non è pronta?

## Opzioni considerate

| Domanda | Opzioni |
|---|---|
| Forma degli indirizzi | **A.** risorse al plurale sotto `/api` (`/api/games/3498`) · **B.** verbi (`/api/getGame?id=3498`) |
| Nomi dei parametri | **A.** nostri, camelCase (`text`, `genreIds`, `pageSize`) · **B.** quelli di RAWG (`search`, `genres`, `page_size`) |
| Filtri con più valori | **A.** ripetuti (`?genreIds=4&genreIds=51`) · **B.** con la virgola (`?genreIds=4,51`) |
| Errori | **A.** ProblemDetails con `400`, `404`, `502` · **B.** testo libero |
| `DeveloperIds`, `PublisherIds`, `StoreIds` in `SearchFilter` | **A.** toglierli · **B.** aggiungerli al contratto |

## Decisione

<!-- Per ogni domanda: opzione scelta e perché, in una riga. Poi la tabella degli endpoint concordati. -->

| Richiesta | Risposta |
|---|---|
| | |

### Conseguenze

<!-- Cosa cambia per A (backend) e per B (client); cosa diventa più facile e cosa più difficile. -->

## Fonti

- [Model binding in ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/models/model-binding) · [ProblemDetails](https://learn.microsoft.com/aspnet/core/web-api/handle-errors) · [RAWG API](https://api.rawg.io/docs/)
