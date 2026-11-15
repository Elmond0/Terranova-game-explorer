# ADR-07 — Dati dei giochi: da RAWG a IGDB

- **Stato:** Accettato
- **Data:** 06/11/2026
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

La traccia chiede di prendere i giochi dalle API di RAWG, e fino al 5 novembre la Web API lo faceva. All'inizio di novembre RAWG ha smesso di rispondere in modo affidabile e i docenti hanno indicato IGDB come alternativa, con una guida per l'integrazione (`DOCS/Guida per integrazione con IGDB.pdf`).

Mancava circa una settimana alla consegna. Dovevamo decidere se e come cambiare fonte senza riscrivere il client e senza perdere le funzioni già fatte.

## Opzioni considerate

- **A. Restare su RAWG** e gestire meglio gli errori quando non risponde.
- **B. Passare a IGDB** cambiando solo il servizio della Web API e tenendo uguali i modelli condivisi e gli endpoint.
- **C. Usare tutte e due**, IGDB come riserva quando RAWG non risponde.

## Decisione

**B. Passiamo a IGDB, il contratto con il client non cambia.**

A non era praticabile: con RAWG giù l'app non mostrava niente, e nessuna gestione degli errori risolve questo. C voleva dire mantenere due servizi con id, campi e regole diverse per lo stesso gioco, con una settimana a disposizione.

La separazione che avevamo già ci ha aiutato: il client parla solo con la nostra Web API e usa i modelli di `shared/`. Abbiamo riscritto la parte che chiama il servizio esterno (`Services/`) e tenuto uguali gli indirizzi (`/api/games`, `/api/genres`, `/api/studios/…`) e la forma del JSON. Il client ha avuto bisogno solo di piccoli ritocchi.

### Che cosa cambia rispetto a RAWG

| Cosa | RAWG | IGDB |
|---|---|---|
| Accesso | chiave nell'indirizzo (`?key=`) | Client ID e Client Secret di Twitch, da cui la Web API ottiene un token |
| Richieste | `GET` con parametri | `POST` con una query nel corpo (linguaggio Apicalypse: `fields …; where …; sort …; limit …;`) |
| Voto | `rating` da 0 a 5 e `metacritic` | `rating` (utenti) e `aggregated_rating` (critica) da 0 a 100; nessun Metacritic |
| Screenshot | endpoint separato | dentro la richiesta del gioco, quindi la scheda li riceve già (`GameDetails.Screenshots`) |
| Ricerca per nome | `search` con un suo ordine di pertinenza | filtriamo con `name ~ *"testo"*` anche sui nomi alternativi e ordiniamo noi (di default per numero di voti, cioè per popolarità) |
| Id dei giochi | id di RAWG | id di IGDB, diversi: i giochi salvati prima del cambio non valgono più |

### Conseguenze

- **Segreti:** due file nuovi in `secrets/` (`twitch_id.txt`, `twitch_key.txt`), passati alla Web API come `Igdb__ClientId` e `Igdb__ClientSecret`. La Web API non parte se mancano (`ValidateOnStart`). Il Client Secret non arriva mai al client.
- **Token:** lo chiede `TwitchTokenProvider` e lo aggiunge a ogni richiesta `IgdbAuthHandler`; resta in cache, così non lo chiediamo a ogni chiamata.
- **Database:** la colonna `rawg_id` è diventata `game_id` e i giochi salvati con gli id di RAWG sono stati cancellati (migrazione `RenameRawgIdToGameId`). La colonna `metacritic` è stata tolta e il voto è passato alla scala 0-100 (`RemoveMetacriticRatingTo100`).
- **Client:** il voto si mostra su 100.
- **Metacritic:** la traccia lo chiede nella scheda e i docenti, nelle risposte del 4 novembre, lo indicano come filtro di valutazione. IGDB non lo fornisce. L'equivalente più vicino è `aggregated_rating`, la media dei voti della critica. **Da confermare con i docenti** prima di usarlo con il nome "Metacritic".
- **Giochi da mostrare:** IGDB contiene anche DLC, mod, episodi e bundle. Nelle ricerche teniamo solo giochi principali, espansioni, remake, remaster, porting e simili (`game_type`), altrimenti i risultati si riempiono di contenuti minori.
- **Postman:** la collection `GameExplorer-Games-IGDB` contiene sia le richieste dirette a IGDB sia quelle alla nostra Web API. Le collection di RAWG restano solo come storico.

## Fonti

- [Documentazione IGDB](https://api-docs.igdb.com/) · [Autenticazione IGDB con Twitch](https://api-docs.igdb.com/#authentication) · [Client credentials flow di Twitch](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#client-credentials-grant-flow) · [Documentazione RAWG](https://rawg.io/apidocs) · `DOCS/qa-and-igdb-guide.md` (guida e risposte dei docenti)
