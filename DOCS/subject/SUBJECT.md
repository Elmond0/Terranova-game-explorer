# Traccia: Terranova Software, Challenge Episode 3, "Gaming Explorer"

> Trascrizione strutturata di `it.game_explorer.pdf` (23 pagine, 14/10/2026). Il PDF inglese `en.game_explorer.pdf` ha lo stesso contenuto.
> Le figure del PDF sono solo decorative e non aggiungono requisiti.

## In una riga
Web app client-server per esplorare i videogiochi tramite le **API REST di RAWG**. Backend **ASP.NET Core**, frontend **Angular + Bootstrap** (ammessi altri framework se si motiva la scelta). Si lavora **in gruppo**.

## Obiettivi formativi (cap. II)
- Architettura client-server moderna e modulare
- API REST con ASP.NET Core (o equivalente)
- UI dinamica e responsive con Angular, TypeScript e Bootstrap (o equivalente)
- Consumo di API REST esterne (JSON)
- Ricerca avanzata, filtri, ordinamento, navigazione
- Persistenza su DB relazionale
- Buone pratiche: modularità, riuso, separazione delle responsabilità
- Git per versionamento e collaborazione del gruppo
- Documentare il progetto e **motivare le scelte**
- Test delle API con Postman
- Uso dell'AI come supporto, **capendo il codice prodotto**

Vengono valutati anche la qualità complessiva, **il lavoro di gruppo** e la padronanza delle tecnologie, non solo la completezza.

## Funzionalità obbligatorie (cap. IV.1)
| # | Funzionalità | Requisiti |
|---|---|---|
| 1 | **Home Page** | Panoramica dell'app, barra di ricerca, selezione di giochi presa dalle API. È il punto di accesso principale |
| 2 | **Ricerca base** | Per nome, anche parziale. Interroga le API e mostra i risultati in modo dinamico |
| 3 | **Ricerca avanzata** | Filtri: genere, piattaforma, anno di pubblicazione, rating, ordinamento (nome, data, rating). Aggiornamento automatico o tramite un pulsante |
| 4 | **Elenco** | Lista o griglia di card. Per ogni gioco: copertina, titolo, data di uscita, rating medio, piattaforme principali. Ogni card è cliccabile |
| 5 | **Dettaglio** | Pagina dedicata con descrizione, immagini/screenshot, generi, piattaforme, sviluppatore, publisher, rating, **Metacritic** |
| 6 | **Preferiti** | Aggiungi/rimuovi, sezione dedicata. Si possono salvare nel DB (oppure nel localStorage) |
| 7 | **Stati dell'app** | Indicatore di caricamento, gestione degli errori delle API, messaggio "nessun risultato" |
| 8 | **Responsive** | Desktop, tablet, smartphone |

## Endpoint RAWG indicati (cap. IV.2)
| Endpoint | Uso |
|---|---|
| `GET /games` | Lista con ricerca, filtri, ordinamento, paginazione |
| `GET /games/{id}` | Dettaglio |
| `GET /games/{id}/screenshots` | Screenshot |
| `GET /genres` | Generi |
| `GET /platforms` | Piattaforme |

Parametri di `/games`: `search`, `genres`, `platforms`, `dates`, `ordering`, `metacritic`, `page`, `page_size`. L'elenco è dichiarato "non esaustivo".

## Specifiche tecniche (cap. V)
- **Backend**: C# + ASP.NET Core Web API (o altro, motivando). Riceve le richieste del frontend, chiama le API esterne, elabora i dati ed espone endpoint REST propri. IDE suggerito: Visual Studio Community (altri ammessi, motivando).
- **Frontend**: Angular + TypeScript, Bootstrap consigliato. VS Code, Node.js, npm. Altri framework ammessi, motivando.
- **Database**: SQL Server Express consigliato. Alternative: PostgreSQL, MySQL, SQLite, **motivando la scelta nella documentazione**. Per i dati solo lato client (es. preferiti) è ammesso il localStorage.
- **Git**: obbligatorio. Repo condiviso, preferibilmente su GitHub, con commit frequenti e descrittivi.
- **Postman**: tutte le API del backend vanno testate **prima** dell'integrazione col frontend: endpoint, status code, JSON, validazione dei parametri, casi di errore. Consigliata una **Collection Postman** da consegnare come documentazione.

## Uso dell'AI (cap. VI)
Consentito e incoraggiato (analisi, architettura, codice, debug, documentazione), purché si verifichi tutto. **Durante la presentazione ogni membro del gruppo deve saper**:
- descrivere l'architettura
- spiegare come funzionano le feature
- illustrare il codice
- motivare le scelte
- **modificare il codice al momento, se il docente lo chiede**

L'uso dell'AI non dà punti né toglie punti.

## Bonus (cap. VII)
Contano solo se integrati bene, documentati e coerenti con l'architettura:
1. **Autenticazione**: registrazione, login, area personale (ASP.NET Core Identity, eventualmente JWT)
2. **Dark/Light mode** con la preferenza salvata
3. **Confronto** tra 2 o più giochi in un'unica schermata
4. **Wishlist** separata dai preferiti
5. **Statistiche personali**: giochi consultati, numero di preferiti, distribuzione per genere e per piattaforma, giochi con il rating più alto

Si possono proporre altri bonus, ma vanno **concordati con il docente**.

## Consegna (cap. VIII)
1. Codice su GitHub con una cronologia dei commit significativa
2. Documentazione tecnica: feature, architettura, tecnologie, scelte progettuali
3. Presentazione di **circa 10 minuti**: caratteristiche, difficoltà incontrate, soluzioni. Possono essere fatte domande a ciascun membro

## Criteri di valutazione
| Criterio | Punti |
|---|---|
| Correttezza e completezza delle funzionalità obbligatorie | **10** |
| Qualità del codice (organizzazione, modularità, leggibilità, buone pratiche) | **10** |
| Interfaccia grafica, usabilità, responsive | **10** |
| Documentazione tecnica e repository Git | 5 |
| Presentazione e capacità di spiegare le scelte | 5 |
| Bonus | **+2 al massimo** |

I bonus vengono considerati **solo se le funzionalità obbligatorie sono complete e corrette**. Viene premiata un'app stabile, ordinata, facile da usare, con codice chiaro e documentato.

## Appendice: tutti gli endpoint RAWG (non fa parte della traccia)
Ricavati dallo schema OpenAPI ufficiale (`https://api.rawg.io/docs/?format=openapi`), verificato il 2026-10-20. URL di base `https://api.rawg.io/api`, `?key=` obbligatorio su ogni chiamata, tutti in GET.

| Endpoint | Cosa restituisce | Query |
|---|---|---|
| `/games` | Lista giochi | `page`, `page_size`, `search`, `search_precise`, `search_exact`, `parent_platforms`, `platforms`, `stores`, `developers`, `publishers`, `genres`, `tags`, `creators`, `dates`, `updated`, `platforms_count`, `metacritic`, `exclude_collection`, `exclude_additions`, `exclude_parents`, `exclude_game_series`, `exclude_stores`, `ordering` |
| `/games/{id}` | Dettaglio | — |
| `/games/{id}/screenshots` | Screenshot | `ordering`, paginazione |
| `/games/{id}/movies` | Trailer | — |
| `/games/{id}/stores` | Link agli store che lo vendono | `ordering`, paginazione |
| `/games/{id}/additions` | DLC, edizioni GOTY, app companion | paginazione |
| `/games/{id}/parent-games` | Gioco "padre" di un DLC o di un'edizione | paginazione |
| `/games/{id}/game-series` | Altri giochi della stessa serie | paginazione |
| `/games/{id}/development-team` | Persone del team di sviluppo | `ordering`, paginazione |
| `/games/{id}/achievements` | Achievement (con `percent` di sblocco) | — |
| `/games/{id}/reddit` | Post recenti del subreddit | — |
| `/games/{id}/suggested` | Giochi visivamente simili | 🔒 solo business/enterprise |
| `/games/{id}/twitch` | Stream Twitch | 🔒 solo business/enterprise |
| `/games/{id}/youtube` | Video YouTube | 🔒 solo business/enterprise |
| `/genres`, `/genres/{id}` | Generi | `ordering`, paginazione |
| `/platforms`, `/platforms/{id}` | Piattaforme | `ordering`, paginazione |
| `/platforms/lists/parents` | Famiglie di piattaforme (es. PlayStation per PS2/PS4) | `ordering`, paginazione |
| `/stores`, `/stores/{id}` | Store digitali | `ordering`, paginazione |
| `/developers`, `/developers/{id}` | Sviluppatori | paginazione |
| `/publishers`, `/publishers/{id}` | Publisher | paginazione |
| `/tags`, `/tags/{id}` | Tag | paginazione |
| `/creators`, `/creators/{id}` | Persone (game designer, compositori…) | paginazione |
| `/creator-roles` | Ruoli dei creator | paginazione |

Valori di `ordering` su `/games`: `name`, `released`, `added`, `created`, `updated`, `rating`, `metacritic`, con `-` davanti per l'ordine inverso.

## Riferimenti citati
Visual Studio / VS Code / GitHub, SQL Server Express, Postman, npm, GDPR (garanteprivacy.it), [RAWG API docs](https://rawg.io/apidocs), guida VS Code + TypeScript (Kinsta).
