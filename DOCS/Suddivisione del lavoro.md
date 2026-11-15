# Suddivisione del lavoro – Guida ai videogiochi

Chi fa cosa, cosa è fatto e cosa manca. Regole su branch, commit, PR e cartelle di ognuno: [`git-workflow.md`](git-workflow.md). Traccia: [`subject/SUBJECT.md`](subject/SUBJECT.md). Avvio: [`README.md`](../README.md).

> **Ci fermiamo alla fase 2.** La pubblicazione online non è richiesta (risposta 12 del 04/11): il progetto gira in locale. Prezzi, calendario, tier list, Steam, DNA, Wrapped, difficoltà, Reddit e classifica sono stati tolti.

## Architettura

Stack in [ADR-01](adr/adr-01-stack.md): due app e un database.

```
Browser ⇄ Client Blazor WebAssembly ──HTTP──▶ Web API ASP.NET Core ──▶ PostgreSQL
                                                    └──────────────▶ IGDB (via Twitch)
```

| Progetto | Cartella | Contenuto |
| --- | --- | --- |
| Web API | `be/GameExplorer.Api` | Endpoint, servizio IGDB, cache, login, Entity Framework |
| Client | `fe/GameExplorer.Client` | Pagine e componenti Blazor con MudBlazor, client HTTP verso la Web API |
| Shared | `shared/GameExplorer.Shared` | DTO scambiati tra le due app: il contratto |

- Solo la Web API parla con il database e con IGDB.
- Nel client niente chiavi né segreti: chiunque può scaricarlo.
- Ogni pagina con dati di IGDB cita IGDB come fonte.

## Ruoli

| Persona | Area | Cosa ha fatto |
| --- | --- | --- |
| **A** – [`@K0ff33Blu3`](https://github.com/K0ff33Blu3) | Web API e servizi | Soluzione, servizio IGDB (prima RAWG), endpoint di giochi, generi, piattaforme e studi, cache, gestione degli errori, test e Postman dei giochi |
| **B** – [`@fmartini02`](https://github.com/fmartini02) | Client | Layout, tema chiaro/scuro salvato, home, ricerca, scheda, studio, profilo, pulsanti preferiti/wishlist, pagine di login e registrazione, test bUnit |
| **C** – [`@Elmond0`](https://github.com/Elmond0) | Database e account | PostgreSQL e Docker, Makefile, migrazioni, login con cookie, preferiti, wishlist, profilo e cancellazione account, CI del database, test e Postman dell'account |

## Percorso fatto

Le scelte cambiate strada facendo sono negli ADR.

### Fase 0 – Preparazione ✅

- [x] **A** – Soluzione con i tre progetti, CORS, `HttpClient` del client verso la Web API
- [x] **B** – Tema MudBlazor (scuro come base), layout, card di un gioco
- [x] **C** – PostgreSQL con `compose.yaml` e `make`, utenti del database separati (admin, migrazioni, Web API), Entity Framework con Npgsql, `secrets/` passati dal Makefile
- [x] **Tutti** – MudBlazor come libreria grafica

### Fase 1 – Home, ricerca, scheda e studio ✅

- [x] **A** – Servizio IGDB: ricerca con filtri e ordinamento, dettaglio, liste della home, studi, cache in memoria. Passaggio da RAWG a IGDB in [ADR-07](adr/adr-07-da-rawg-a-igdb.md)
- [x] **A** – Endpoint `api/games`, `api/genres`, `api/platforms`, `api/studios` con i modelli di Shared ([ADR-02](adr/adr-02-contratto-api.md), [ADR-03](adr/adr-03-organizzazione-codice.md))
- [x] **B** – `IGameApiClient` con versione a dati finti, home, ricerca, scheda, pagina studio, stati di caricamento, errore e nessun risultato
- [x] **C** – Tabella `game_snapshots`, poi tolta: salviamo solo gli id e le card arrivano da IGDB ([ADR-06](adr/adr-06-account-preferiti.md))

### Fase 2 – Account, preferiti e wishlist ✅

- [x] **C** – Login con cookie scritto da noi al posto di ASP.NET Core Identity ([ADR-04](adr/adr-04-login-cookie.md)), tabelle ([ADR-05](adr/adr-05-database.md)) ed endpoint protetti `api/me` per preferiti, wishlist, profilo e cancellazione account
- [x] **B** – Pagine di login e registrazione, `AuthenticationStateProvider`, pulsanti preferiti/wishlist, pagina profilo
- [x] **B** – Tema chiaro/scuro ricordato tra le visite
- [x] **A** – Errori di IGDB centralizzati con `ProblemDetails`

### Bonus

Contano solo con gli obbligatori completi, al massimo 2 punti.

- [x] Autenticazione, tema scuro salvato, wishlist separata, profilo
- [ ] **C + B** – Statistiche personali (tabella, endpoint, pagina con grafici)
- [ ] **B** – Confronto tra 2 o 3 giochi

## Regole di lavoro

- Un branch per compito, PR rivista da un altro membro, mai su `main` direttamente.
- Nelle cartelle degli altri si entra solo con una PR piccola rivista dal proprietario.
- Solo C crea le migrazioni; si applicano con `db/scripts/migrate.sh`, mai all'avvio della Web API.
- I modelli di Shared si cambiano solo d'accordo tra chi li usa.
- Le decisioni prese insieme vanno in un ADR in `DOCS/adr/`.
- Password e chiavi solo in `secrets/` (istruzioni in [`db/README.md`](../db/README.md)), mai nel codice, nel client o su GitHub.

## Fonti

- Microsoft: [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) · [Web API](https://learn.microsoft.com/aspnet/core/web-api/) · [EF Core](https://learn.microsoft.com/ef/core/) · [CORS](https://learn.microsoft.com/aspnet/core/security/cors)
- [IGDB](https://api-docs.igdb.com/) · [MudBlazor](https://mudblazor.com/)
