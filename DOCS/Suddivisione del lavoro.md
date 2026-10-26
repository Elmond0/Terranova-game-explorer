# Suddivisione del lavoro – Guida ai videogiochi

Chi fa cosa, fase per fase. Le regole su branch, commit e Pull Request sono in [`DOCS/come_lavoriamo.md`](come_lavoriamo.md); la traccia è in [`DOCS/subject/SUBJECT.md`](subject/SUBJECT.md).

## Architettura

Lo stack è deciso in [ADR-01](adr/adr-01-stack.md). Il progetto è formato da **due app distinte e un database unico**:

```
Browser ⇄ Client Blazor WebAssembly ──HTTP──▶ Web API ASP.NET Core ──▶ PostgreSQL
                                                    ├──────────────▶ RAWG
                                                    └──────────────▶ Steam, CheapShark, Reddit
```

| Progetto | Cartella | Cosa contiene |
| --- | --- | --- |
| Web API | `be/GameExplorer.Api` | Endpoint REST, servizi verso le API esterne, cache, calcoli, Entity Framework, login |
| Client | `fe/GameExplorer.Client` | Pagine e componenti Blazor WebAssembly con MudBlazor, client HTTP verso la Web API |
| Shared | `shared/GameExplorer.Shared` | Modelli (DTO) scambiati tra API e client: è il "contratto" tra le due app |

Regole che derivano da questa scelta:

- **Solo la Web API accede al database e alle API esterne.** Il client parla solo con la Web API.
- **Il client WebAssembly gira nel browser e chiunque può scaricarlo e leggerlo:** niente chiavi, password o segreti nel client, neanche in `wwwroot/appsettings.json`.
- **Il contratto tra le due app sono gli endpoint e i modelli in Shared**, non i metodi dei servizi.
- **Ogni pagina che mostra dati di RAWG ha un link a RAWG**: lo chiedono le [condizioni d'uso](https://rawg.io/apidocs) del piano gratuito.

## Ruoli

Ognuno ha un'area di cui è responsabile, ma in ogni fase tutti e tre lavorano: così nessuno resta fermo ad aspettare gli altri e alla fine tutti conoscono ogni parte del progetto.

| Persona | Area di responsabilità | Cosa fa in pratica |
| --- | --- | --- |
| **A** – [`@K0ff33Blu3`](https://github.com/K0ff33Blu3) | Dati e servizi (Web API) | Imposta la soluzione, scrive nella Web API i servizi che chiamano le API esterne (RAWG, prezzi, Steam, Reddit), gli endpoint, la cache e i calcoli (DNA, indice di difficoltà) |
| **B** – [`@fmartini02`](https://github.com/fmartini02) | Interfaccia e grafica (client) | Pagine e componenti Blazor, client HTTP verso la Web API, stile, tema chiaro/scuro, versione per smartphone, animazioni, tier list e Steam Wrapped |
| **C** – [`@Elmond0`](https://github.com/Elmond0) | Database e account (Web API e pagine dell'account nel client) | PostgreSQL, Entity Framework e migrazioni, registrazione e login nella Web API, accesso con Steam, salvataggio di preferiti, wishlist e tier list, pubblicazione online |

Partite tutti e tre da C e C++, quindi le tre aree sono equivalenti come difficoltà. A prende i servizi perché con TypeScript ha già visto concetti tipici del web, come JSON, chiamate HTTP e async/await, e può aiutare gli altri anche con i pochi file JavaScript del progetto. Se preferite un'altra divisione, scambiatevi pure: conta che ogni area abbia un responsabile.

## Fase 0 – Preparazione

**Fatto quando:** tutti e tre scaricano il progetto da GitHub, avviano database, Web API e client sul proprio computer, si registrano tramite la Web API (file `.http` o Postman) e vedono la pagina base del client con lo stile scelto, che riceve dati dalla Web API.

**Tutti**

- [ ] Installare l'SDK di .NET 10, Visual Studio o VS Code con C# Dev Kit, Git, Docker
- [ ] Seguire insieme un tutorial introduttivo di Blazor WebAssembly e uno di ASP.NET Core Web API (per esempio quelli ufficiali di Microsoft)
- [x] Scegliere la libreria grafica: **MudBlazor** (vedi ADR-01)
- [ ] Chiedere al docente se le funzioni extra (prezzi, calendario, tier list, Steam e DNA, Wrapped, difficoltà, Reddit, classifica) valgono come bonus: la traccia vuole che i bonus fuori dal suo elenco siano concordati con lui

**A – Dati e servizi**

- [ ] Creare la soluzione con i tre progetti: `be/GameExplorer.Api` (Web API), `fe/GameExplorer.Client` (Blazor WebAssembly standalone), `shared/GameExplorer.Shared`, tutti nel file `GameExplorer.slnx`
- [ ] Aggiungere il riferimento a Shared sia nella Web API sia nel client
- [ ] Preparare le cartelle: nella Web API `Controllers`, `Services`, `Data` (il codice del database, gestito da C); nel client `Pages`, `Layout`, `Shared`, `Services`; in Shared `Models`
- [ ] Configurare il CORS nella Web API, per accettare le chiamate dall'indirizzo del client
- [ ] Nel client, mettere l'indirizzo della Web API di sviluppo in `wwwroot/appsettings.Development.json` e registrare l'`HttpClient` con quell'indirizzo (sul sito pubblicato client e Web API hanno lo stesso indirizzo)
- [ ] Richiedere le chiavi di RAWG e Steam Web API e salvarle negli User Secrets della **Web API**

**B – Interfaccia e grafica**

- [ ] Scegliere palette, font e stile generale (tema scuro come base) e configurarli nel tema di MudBlazor
- [ ] Creare il layout: intestazione, menu, piè di pagina, selettore tema chiaro/scuro
- [ ] Disegnare la card di un gioco con dati finti

**C – Database e account**

- [x] Avviare PostgreSQL con il `compose.yaml` del progetto (istruzioni in `db/README.md`)
- [ ] Creare l'utente del database per la Web API, che legge e scrive i dati ma non può modificare le tabelle
- [ ] Aggiungere Entity Framework Core con Npgsql alla Web API (cartella `Data`; i pacchetti con una PR piccola rivista da A) e la stringa di connessione negli User Secrets
- [ ] Creare la prima migrazione, applicarla con `db/scripts/migrate.sh` e configurare ASP.NET Core Identity nella Web API (endpoint di registrazione e login), verificandola con il file `.http` o Postman
- [ ] Creare il database online gratuito per la versione pubblicata: Neon, perché Supabase gratuito sospende i progetti dopo 7 giorni senza attività

## Compiti per fase

Alla fine di ogni fase il sito funziona: se il tempo finisce, vi fermate lì con un progetto completo.

### Fase 1 – Home, ricerca e scheda gioco

**Fatto quando:** si cerca un gioco con i filtri, si apre la sua scheda e da lì la pagina dello studio.

- [ ] **A** – RawgService nella Web API: ricerca con filtri e ordinamento, dettaglio gioco con screenshot, liste della home, studi
- [ ] **A** – Endpoint della Web API per tutte queste funzioni, con i modelli di Shared (`Game`, `GameDetails`, `Studio`, `Platform`, `Genre`, `PagedResult`, `SearchFilter`) e cache in memoria delle risposte di RAWG
- [ ] **B** – Client HTTP verso la Web API (`IGameApiClient`) e una sua versione con dati finti
- [ ] **B** – Home con barra di ricerca e selezioni (più apprezzati, recenti, in arrivo)
- [ ] **B** – Pagina ricerca: risultati mentre si scrive, filtri, ordinamento
- [ ] **B** – Scheda gioco: descrizione, galleria, generi, piattaforme, voti
- [ ] **B** – Stati comuni: caricamento, nessun risultato, errore
- [ ] **C** – Pagina casa produttrice nel client (numero di giochi, voto medio, elenco), con i dati dall'endpoint degli studi e i componenti di B
- [ ] **C** – Tabella di cache dei giochi nel database, per ridurre le chiamate a RAWG, con una piccola interfaccia che il RawgService di A usa per leggerla e scriverla

### Fase 2 – Account, preferiti e wishlist

**Fatto quando:** un utente registrato salva giochi in preferiti e wishlist e li ritrova da un altro dispositivo.

- [ ] **C** – Tabelle, servizio ed endpoint protetti (solo utenti autenticati) per preferiti e wishlist
- [ ] **C** – Pagine di registrazione e login nel client, tradotte, collegate al menu e agli endpoint di Identity della Web API (stato di autenticazione nel client con un `AuthenticationStateProvider`), più l'uscita (logout), che Identity non crea da solo
- [ ] **B** – Pulsanti preferiti/wishlist nella scheda gioco
- [ ] **B** – Pagina profilo con le due liste e stile delle pagine di login
- [ ] **A** – Tema chiaro/scuro ricordato tra le visite (salvato nel browser)
- [ ] **A** – Gestione centralizzata degli errori nella Web API (una fonte esterna giù non blocca il sito, risposte di errore coerenti con `ProblemDetails`)

### Fase 3 – Prezzi, calendario e tier list

**Fatto quando:** la scheda mostra i prezzi PC, il calendario conta i giorni alle uscite e la tier list viene salvata.

- [ ] **A** – PrezziService (CheapShark) nella Web API, con endpoint: prezzi per negozio, sconti, link, caso "non disponibile su PC"
- [ ] **A** – Prezzo migliore accanto a ogni gioco della wishlist
- [ ] **B** – Riquadro prezzi nella scheda gioco
- [ ] **B** – Calendario uscite raggruppato per mese, con conto alla rovescia
- [ ] **B** – Tier list con trascinamento nelle fasce S, A, B, C, D
- [ ] **C** – Salvataggio della tier list per utente (tabella ed endpoint)

### Fase 4 – Steam e DNA del giocatore

**Fatto quando:** un utente collega Steam e vede il grafico dei suoi generi con tre giochi consigliati.

- [ ] **C** – Collegamento dell'account a Steam (OpenID) gestito dalla Web API, salvataggio dello SteamID, cancellazione dell'account e di tutti i dati su richiesta
- [ ] **A** – SteamService nella Web API: giochi posseduti e ore, generi di ogni gioco, rilevamento profilo privato
- [ ] **A** – Calcolo del DNA nella Web API (percentuali per genere pesate sulle ore) e giochi consigliati
- [ ] **B** – Pagina DNA con grafico e card dei consigliati
- [ ] **B** – Messaggio con istruzioni per rendere pubblico il profilo Steam

### Fase 5 – Wrapped, difficoltà, Reddit e classifica

**Fatto quando:** tutte le funzioni del documento di progetto sono online e testate.

- [ ] **A** – Indice di difficoltà nella Web API (percentuali degli obiettivi Steam, durata, tag) con testo esplicativo
- [ ] **A** – RedditService nella Web API: ultimi post della community del gioco
- [ ] **B** – Steam Wrapped: slide animate e immagine finale da salvare
- [ ] **B** – Indice di difficoltà e post Reddit nella scheda gioco
- [ ] **C** – Classifica della community (media delle fasce di tutti gli utenti): tabella ed endpoint
- [ ] **B** – Pagina della classifica della community
- [ ] **A** – Dati del Wrapped nella Web API (ore totali, gioco e genere preferiti, titolo) e `Dockerfile` che pubblica Web API e client in un'unica immagine
- [ ] **C** – Pubblicazione online: database su Neon e un solo servizio su Render con l'immagine di A, in cui la Web API serve anche il client. Stesso indirizzo per tutti e due: niente CORS in produzione e il cookie del login non è di terze parti
- [ ] **Tutti** – Prova completa su computer, tablet e smartphone del sito pubblicato
- [ ] **Tutti** – Documentazione tecnica da consegnare (funzionalità, architettura, tecnologie, scelte progettuali), motivazioni nell'ADR e collection Postman completa
- [ ] **Tutti** – Presentazione di circa 10 minuti: ognuno sa spiegare e modificare anche le parti degli altri

### Bonus della traccia (solo dopo gli obbligatori)

La traccia conta i bonus solo se le funzionalità obbligatorie sono complete e corrette, e al massimo per 2 punti. Autenticazione, tema scuro salvato e wishlist sono già nelle fasi qui sopra.

- [ ] **C** – Statistiche personali: giochi consultati, numero di preferiti, distribuzione per genere e piattaforma, preferiti con il rating più alto (tabella ed endpoint)
- [ ] **B** – Pagina delle statistiche personali, con i grafici
- [ ] **B** – Confronto tra 2 o 3 giochi in un'unica schermata, con i dati del dettaglio che esistono già

## Dipendenze e punti di incontro

Gli accordi della fase 1 ancora aperti sono raccolti in una issue, "Accordi della fase 1": le decisioni stanno nei suoi commenti.

La regola che evita le attese è questa: **all'inizio di ogni fase vi accordate su quali dati si scambiano le parti**, cioè gli endpoint della Web API (indirizzo, parametri, risposte di errore) e i modelli in Shared. Poi ognuno lavora per conto suo. Chi fa l'interfaccia usa una versione del client HTTP con dati finti finché l'endpoint vero non è pronto.

| Fase | Chi si accorda | Su cosa |
| --- | --- | --- |
| 1 | A e B | Modelli `Game`, `GameDetails`, `SearchFilter`, `PagedResult` ed endpoint (ricerca, dettaglio, liste home, generi, piattaforme) |
| 1 | A e C | Quali dati dei giochi salvare nella cache del database ed endpoint degli studi |
| 2 | C e B | Endpoint per aggiungere, togliere e leggere preferiti e wishlist, e come il client si autentica |
| 3 | B e C | Formato della tier list salvata (gioco + fascia) e relativo endpoint |
| 3 | A e B | Dati del prezzo (negozio, prezzo, sconto, link) e relativo endpoint |
| 4 | C e A | Dove si trova lo SteamID dell'utente collegato |
| 4 | A e B | Dati del DNA (genere + percentuale) e lista dei consigliati |
| 5 | A e B | Dati del Wrapped (ore totali, genere preferito, gioco preferito, titolo) |
| 5 | C e B | Dati della classifica della community |
| 5 | A e C | `Dockerfile` e variabili d'ambiente per la pubblicazione |

## Regole di lavoro

Poche regole fisse evitano la maggior parte dei conflitti in un gruppo di tre.

- **Un branch per ogni compito** (per esempio `ricerca-filtri`), mai lavorare direttamente su `main`.
- **Pull request revisionata da un altro membro** prima di unirla a `main`: è anche il modo per imparare le parti degli altri.
- **Solo C crea le migrazioni del database**: due migrazioni create in parallelo vanno quasi sempre in conflitto. Chi ha bisogno di una nuova tabella lo chiede a C. Le migrazioni si applicano solo con `db/scripts/migrate.sh`, mai all'avvio della Web API.
- **I modelli in Shared si cambiano solo d'accordo** tra chi li usa: sono il contratto tra Web API e client.
- **Le decisioni prese insieme si scrivono in un ADR** in `DOCS/adr/` (contesto, decisione, alternative, conseguenze), con una PR rivista dagli altri due. È la documentazione delle scelte che la traccia chiede di motivare.
- **Nei file degli altri si entra con una PR piccola** che il proprietario rivede (per esempio i pacchetti del database nel progetto della Web API, o le pagine di C nel client). Le cartelle di ognuno sono nella tabella iniziale di `DOCS/come_lavoriamo.md`.
- **Chiavi delle API solo negli User Secrets della Web API** (in produzione nelle variabili d'ambiente), mai nel codice, nel client o su GitHub.
- **Commit piccoli e frequenti**, con messaggi chiari ("Aggiunti filtri per piattaforma").
- **Elenco dei compiti su GitHub Projects**: ogni voce di questo documento diventa una scheda, con colonne Da fare, In corso, Fatto.
- **Riunione breve una volta a settimana**: cosa ho finito, cosa faccio ora, dove sono bloccato.
- **Fine di ogni fase**: dimostrazione insieme del sito funzionante, poi si accordano i dati della fase successiva.

## Fonti

- Traccia del progetto: [`DOCS/subject/SUBJECT.md`](subject/SUBJECT.md) · stack: [ADR-01](adr/adr-01-stack.md)
- Microsoft: [Blazor WebAssembly](https://learn.microsoft.com/aspnet/core/blazor/) · [Web API con ASP.NET Core](https://learn.microsoft.com/aspnet/core/web-api/) · [EF Core](https://learn.microsoft.com/ef/core/) · [Blazor WebAssembly con Identity](https://learn.microsoft.com/aspnet/core/blazor/security/webassembly/standalone-with-identity/) · [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) · [CORS](https://learn.microsoft.com/aspnet/core/security/cors)
- Servizi esterni: [RAWG](https://rawg.io/apidocs) · [CheapShark](https://apidocs.cheapshark.com/) · [Steam Web API](https://developer.valvesoftware.com/wiki/Steam_Web_API) · [Neon, piani](https://neon.com/docs/introduction/plans)
- Libreria grafica: [MudBlazor](https://mudblazor.com/)
