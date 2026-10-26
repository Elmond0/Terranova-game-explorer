# ADR-02 — Contratto delle API

- **Stato:** Accettato
- **Data:** 26/10/2026
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

Client e Web API li scrivono persone diverse. Come sono fatti indirizzi, parametri, risposte ed errori della Web API, così che il client possa lavorare con dati finti finché la Web API non è pronta?

## Opzioni discusse

- Forma degli indirizzi
- Nomi nel Json dell'API
- Gestione dei filtri con piu valori
- Gestioni errori chiamate client-server
- Prima stesura contratto endpoint

## Decisione

| Richiesta | Decisione |
|---|---|
| Forma degli indirizzi | risorse al plurale `/api/games/xxxx` (vedi contratto) |
| Nomi dei parametri | I nomi dei parametri dei nostri metodi (`GameExplorer/Shared/Models`) in camelCase |
| Filtri con piu valori | liste di id:int |
| Forma indirizzi filtri con piu valori | id separati da virgola, formato `?genreIds=4,51` |
| Errori | ProblemDetails con codici Http |

## Contratto

| Richiesta HTTP | Risposta |
|---|---|
| GET /api/games/{id} |	GameDetails, screenshot compresi |
| GET /api/games/top-rated, /recent, /upcoming | List<Game> |
| GET /api/genres | List<Genre> |
| GET /api/platforms | List<Platform> |
| GET /api/studios/{id} | Studio |
| GET /api/studios/{id}/games?page=&ordering= |	PagedResult<Game> |

### Dettagli

I `GameDetails.WhereToBuy` vengono inviati come oggetto **StoreLink** che contiene il link all'acquisto sullo store **DEL GIOCO** e l'istanza dello `Store`. L'oggetto `Store` ha tra le proprieta `Store.Domain`, che contiene l'URL al sito homepage dello store.

## Da decidere

- [ ] Lunghezza /top-rated /recent /upcoming
- [ ] Pagina genre/{id} (come per studio)?
- [ ] pageSize di default?
- [ ] screenshots in GameDetails oppure /api/games/{id}/screenshots

## Fonti

- [Model binding in ASP.NET Core](https://learn.microsoft.com/aspnet/core/mvc/models/model-binding) · [ProblemDetails](https://learn.microsoft.com/aspnet/core/web-api/handle-errors) · [RAWG API](https://api.rawg.io/docs/)
