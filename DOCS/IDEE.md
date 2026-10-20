# Gaming Explorer: idee e strategia

> Documento di lavoro: lo aggiorniamo man mano che ragioniamo.
> Ultimo aggiornamento: 2026-10-20, **riscritto dopo aver letto la traccia** (vedi [SUBJECT.md](SUBJECT.md)).

---

## 0. Cosa è cambiato rispetto alla prima bozza

La bozza del 16/10 era pensata per un hackathon "vince l'idea più originale". La traccia racconta un'altra storia:

| Ipotesi della bozza | Realtà dalla traccia | Conseguenza |
|---|---|---|
| API sconosciute | **RAWG** (`/games`, `/games/{id}`, screenshot, generi, piattaforme) | Possiamo progettare subito |
| Contano originalità e "effetto wow" | **30 punti su 40** vanno a funzionalità obbligatorie, qualità del codice e UI. I bonus valgono **+2 al massimo** | Prima la base perfetta, poi pochi bonus fatti bene |
| Feature complesse (galassia, SignalR, embedding) | **Ogni membro deve saper spiegare e modificare il codice dal vivo** | Stack semplice e leggibile. Ogni pezzo va capito da tutti |
| Frontend libero | Angular + Bootstrap consigliati (altro ammesso, motivando) | **Angular**: è quello che si aspettano e quello che sanno giudicare |
| Da soli? | **In gruppo**: conta anche il lavoro di squadra e la storia Git | Branch, PR, issue, commit di tutti i membri |
| Demo con i giudici che giocano | Presentazione di circa 10 minuti con domande ai singoli | Demo pulita + architettura + difficoltà affrontate |

Le idee "firma" della vecchia bozza (Game Galaxy, Party Mode con SignalR, ricerca semantica con embedding, chatbot AI) **le togliamo dal piano**: valgono al massimo una frazione di 2 punti, costano settimane, aumentano il rischio di bug sull'obbligatorio e sono difficili da spiegare per tutti. Restano in fondo (§7) solo se avanziamo molto tempo.

---

## 1. Dove sono i punti (e dove li prendiamo)

| Criterio | Punti | Come li prendiamo |
|---|---|---|
| Funzionalità obbligatorie | 10 | Tutte e 8 complete, con **una checklist che spuntiamo voce per voce** (§3) |
| Qualità del codice | 10 | Architettura a strati chiara, DTO, servizi, validazione, gestione errori centralizzata, test, niente codice morto |
| UI, usabilità, responsive | 10 | Design curato e coerente, skeleton loader, stati vuoti illustrati, mobile-first, accessibilità |
| Documentazione e Git | 5 | README con screenshot, diagramma dell'architettura, motivazione delle scelte, collection Postman, issue, PR e commit descrittivi |
| Presentazione | 5 | Scaletta provata, ognuno presenta la sua parte, si risponde sul codice |
| Bonus | +2 | I 5 bonus della traccia (contano di sicuro) più 1-2 idee nostre **concordate col docente** |

---

## 2. Stack consigliato

| Livello | Scelta | Motivazione da scrivere nella documentazione |
|---|---|---|
| Backend | **ASP.NET Core 10 Web API** (LTS fino a novembre 2028), con **Controller** | È il backend richiesto. I Controller sono più facili da spiegare e da navigare delle Minimal API |
| Client RAWG | `HttpClient` tipizzato + `Microsoft.Extensions.Http.Resilience` | Retry e timeout già pronti. L'**API key resta sul server** e non finisce nel browser |
| Cache | `HybridCache` o `IMemoryCache` (generi e piattaforme per 24h, dettagli per qualche ora) | Il piano gratuito di RAWG ha un tetto di richieste mensili. Così le pagine sono anche più veloci |
| Database | **SQL Server Express** + **EF Core 10** | È quello consigliato dalla traccia, quindi non serve giustificarlo. Alternativa: SQLite per evitare l'installazione |
| Auth (bonus) | **ASP.NET Core Identity + JWT** | È esattamente quello che cita la traccia |
| Frontend | **Angular** (ultima stabile: verificare se 21 o 22) con standalone components, **signals**, router con lazy loading, `HttpClient` + interceptor | Consigliato dalla traccia. Signals e zoneless sono il modo moderno di scrivere Angular |
| UI | **Bootstrap 5.3** + Bootstrap Icons | Consigliato. Ha il dark mode nativo con `data-bs-theme`, quindi il bonus tema è quasi gratis |
| API doc | OpenAPI (nativo in .NET 10) + Scalar/Swagger UI + **Collection Postman** esportata nel repo | La Collection Postman è richiesta esplicitamente |
| Test | xUnit (servizi e controller, client RAWG mockato) + qualche test Angular sui servizi | Fa la differenza nella "qualità del codice" |
| CI | GitHub Actions: build + test di backend e frontend a ogni PR | Mostra un workflow professionale e una storia Git pulita |

Da evitare: .NET Aspire, microservizi, Redis, CQRS/MediatR, vector DB. Sono tutte cose che vanno spiegate e difese davanti al docente senza dare punti in più.

### Architettura

```
Angular (pages / components / services / models)
      │  HTTP JSON, JWT
      ▼
ASP.NET Core API
  Controllers → Services → RawgClient (HttpClient + cache)  →  RAWG API
                    └────→ Repository / DbContext (EF Core) →  SQL Server
```

- Il frontend **non chiama mai RAWG direttamente**: passa sempre dal nostro backend. È quello che chiede la traccia (§V.1) e protegge la chiave.
- Il backend restituisce **DTO nostri**, puliti e piccoli, non il JSON grezzo di RAWG. Per esempio la card ha solo id, nome, immagine, data, rating e piattaforme. Questo è il punto da mostrare in presentazione: "il backend aggiunge valore, non è un passacarte".
- Gli errori vengono gestiti centralmente con `ProblemDetails` e status code corretti (400 per parametri non validi, 404 per gioco inesistente, 502/503 se RAWG non risponde). Il frontend ha un interceptor che li trasforma in messaggi per l'utente.
- Cache dei preferiti: nel DB salviamo id, nome, immagine e rating del gioco, così la pagina preferiti si carica senza chiamare RAWG.

---

## 3. Checklist obbligatori (la "definition of done")

- [ ] **Home**: hero con barra di ricerca, 2-3 caroselli presi dalle API ("Più votati", "Usciti di recente", "In arrivo"), accesso rapido ai generi
- [ ] **Ricerca base**: input con debounce (300 ms) e risultati dinamici. Anche nella navbar
- [ ] **Ricerca avanzata**: genere, piattaforma, anno (o intervallo), rating/Metacritic minimo, ordinamento. **Filtri salvati nell'URL** (query params): la ricerca si può condividere e funziona il tasto "indietro"
- [ ] **Elenco**: griglia di card con copertina, titolo, data, rating e icone delle piattaforme. Paginazione o "carica altri"
- [ ] **Dettaglio**: descrizione, galleria di screenshot (lightbox), generi, piattaforme, sviluppatore, publisher, rating, badge Metacritic colorato (verde/giallo/rosso)
- [ ] **Preferiti**: cuore sulla card e nel dettaglio, pagina dedicata, salvataggio su DB
- [ ] **Stati**: skeleton loader, toast/alert di errore con "Riprova", empty state con un'illustrazione e un suggerimento ("Prova a togliere qualche filtro")
- [ ] **Responsive**: testato a 375, 768 e 1440 px. Filtri in un offcanvas su mobile
- [ ] **Attribuzione RAWG**: link attivo a RAWG su ogni pagina che mostra i dati (es. nel footer). **Lo impongono i termini d'uso di RAWG**, e gli altri gruppi se ne dimenticheranno
- [ ] Gestione dei giochi senza immagine, senza Metacritic o senza data (fallback puliti, niente "null")
- [ ] La descrizione arriva in HTML: va sanificata o usato `description_raw`

---

## 4. Bonus della traccia (tutti e 5, in quest'ordine)

| # | Bonus | Costo | Note |
|---|---|---|---|
| 1 | **Dark/Light mode** | Bassissimo | `data-bs-theme` + preferenza in localStorage (o sul profilo se loggato). Rispetta `prefers-color-scheme` al primo accesso |
| 2 | **Autenticazione** | Medio | Identity + JWT, registrazione/login, area personale. Rende i preferiti "per utente" e abilita gli altri bonus |
| 3 | **Wishlist** | Basso | Stessa struttura dei preferiti: un'unica tabella `UserGame` con un campo `ListType` (Favorite/Wishlist). È un buon esempio di riuso da mostrare |
| 4 | **Confronto** | Medio | Barra "confronta" stile e-commerce (fino a 3-4 giochi), tabella affiancata che evidenzia il migliore per rating e Metacritic |
| 5 | **Statistiche personali** | Medio | Giochi consultati (log delle visite nel DB), numero di preferiti, grafici per genere e piattaforma (Chart.js / ng2-charts), top rating |

---

## 5. Bonus nostri (da proporre al docente)

Sono tutte fattibili **solo con i dati che RAWG già fornisce** (verificato sullo schema OpenAPI ufficiale: `api.rawg.io/docs/?format=openapi`) e restano spiegabili. Ne scegliamo **1-2 al massimo**.

### 💡 A. "Cosa gioco stasera?" (consigliata)
L'utente indica **quanto tempo ha** (es. "meno di 10 ore"), piattaforma e umore (genere). Il backend combina i filtri RAWG con il campo `playtime` (ore medie su Steam) e propone 3 giochi, con un pulsante "Un altro!".
- Perché: è logica di business **nel nostro backend**, non un semplice inoltro a RAWG. È anche facile da raccontare nella presentazione.
- Costo: basso-medio.

### 💡 B. "Higher or Lower" sui giochi
Mini-gioco: due copertine, "quale ha il Metacritic più alto?", con punteggio e record personale salvato nel DB.
- Perché: chiude la presentazione in modo memorabile (si può far giocare il docente per 30 secondi). È la versione leggera e senza rischi del "Party Mode" della vecchia bozza.
- Costo: basso. Basta un endpoint che pesca coppie casuali dalla cache.

### 💡 C. Dettaglio "ricco" con gli endpoint extra di RAWG
Nella scheda aggiungiamo: **dove comprarlo** (`/games/{id}/stores`), **DLC ed edizioni** (`/additions`), **altri giochi della serie** (`/game-series`), **trailer** (`/movies`), distribuzione dei voti (`ratings`: exceptional/recommended/meh/skip) in una barra, PEGI/ESRB.
- Perché: rende il dettaglio nettamente superiore a quello degli altri gruppi, a costo basso.
- ⚠️ Da verificare con la nostra chiave: alcuni endpoint (`/suggested`, `/movies`) potrebbero restituire pochi dati o essere riservati ai piani business.

### 💡 D. Esplora per sviluppatore/publisher
Cliccando su "CD Projekt Red" nel dettaglio si apre una pagina con logo, descrizione e tutti i suoi giochi (`/developers/{id}`, filtro `developers` su `/games`).
- Costo: basso. Migliora molto la navigazione "a esplorazione", che è il tema dell'app.

### 💡 E. Profilo "DNA da giocatore"
Un'estensione delle statistiche personali: dai generi e dai tag dei preferiti calcoliamo un **profilo** (radar chart) e un'etichetta ("Esploratore", "Stratega"…), più 5 consigli di giochi con generi e tag simili che l'utente non ha ancora salvato.
- Costo: medio. Tra le "idee firma" della vecchia bozza è l'unica che sopravvive, perché è un'estensione naturale del bonus 5.

**Consiglio**: **C + D** si fanno sicuramente (costano poco e migliorano la UI e l'obbligatorio). Tra le idee "nuove" proponiamo **A** al docente, poi **B** se resta tempo.

---

## 5-bis. Idee innovative (ricerca del 20/10)

Obiettivo: qualcosa che **gli altri gruppi non faranno**, ma che resti fattibile **solo con i dati di RAWG** e spiegabile da tutti. Ho controllato i campi sullo schema OpenAPI ufficiale. Quelli utili, oltre ai soliti:
- `added`: quanti utenti RAWG hanno aggiunto il gioco, cioè la **popolarità**;
- `added_by_status`: suddivisione in owned / playing / beaten / toplay / dropped;
- `ratings`: distribuzione dei voti (exceptional / recommended / meh / skip);
- `playtime`: ore medie di gioco;
- `percent` sugli achievement: % di giocatori che li hanno sbloccati;
- `released`, `tags`, `genres`.

> Nelle risposte reali `/games` include anche `genres`, `tags` e `short_screenshots` (lo schema non li documenta tutti): **da verificare appena abbiamo la chiave**.

### 🔥 1. "Game Match": lo swipe che impara i tuoi gusti
Le card arrivano una alla volta e si trascinano in tre zone: **❤️ Lo amo / 🤔 Forse / ✖️ Non fa per me**. Ogni swipe aggiorna un **profilo di gusto** (pesi su generi e tag) e la card successiva viene scelta in base a quel profilo. Ogni consiglio mostra il **perché**: *"Ti propongo Hades perché hai amato 3 giochi con i tag roguelike e combattimento veloce"*.
- **Perché è diversa**: le app "Tinder per videogiochi" esistono ([Dustpile](https://gameblog.fr/en/news/73099-backlog-tinder-video-game/), [NEXTURN](https://www.indiehackers.com/product/nexturn)), ma in una web app da catalogo non le farà nessuno. NEXTURN fa notare che quasi tutti i sistemi di consigli usano solo i "mi piace": noi usiamo **anche il segnale negativo**.
- **Tecnica**: content-based filtering in C#, cioè un punteggio dato dalla somma dei pesi dei tag in comune. È un algoritmo **spiegabile in 2 minuti alla lavagna**, non una scatola nera. CDK drag&drop o gesture touch in Angular. Lo swipe su ❤️ finisce nei preferiti, quello su 🤔 nella wishlist: **riusa e alimenta i bonus obbligatori**.
- Costo: medio. 🚀🚀🚀🚀

### 🔥 2. "Stasera si gioca": match di gruppo
Si crea una stanza e si condivide il link o il QR. Ogni amico dice **che piattaforme ha** e fa swipe sullo stesso mazzo di giochi. Il mazzo è filtrato sull'**intersezione delle piattaforme** e sui giochi multiplayer/co-op (tag di RAWG). Quando un gioco piace a tutti: **"It's a match!"**, con un'animazione.
- **Perché è diversa**: risolve un problema vero ("cosa giochiamo stasera?"). Esistono solo bot Discord come [ReadyUp](https://top.gg/it/bot/1523022401997373551), nessuna web app. **In presentazione si fa giocare il docente col telefono.**
- **Tecnica**: **SignalR** per i voti in tempo reale. È il "Party Mode" della vecchia bozza, ma utile invece che solo divertente. Versione semplice: polling ogni 2 secondi, senza SignalR.
- Costo: medio-alto. Va fatta solo dopo la 1. 🚀🚀🚀🚀🚀

### 🔥 3. Slider "Mainstream ↔ Gemme nascoste"
Uno slider nella ricerca sposta i risultati dai giochi famosi a quelli **ottimi ma poco conosciuti**. Calcoliamo noi un **"Indice gemma"**: rating alto, `ratings_count` sufficiente perché il voto sia affidabile, `added` basso.
- **Perché è diversa**: è il concetto dell'[Interactive Recommender di Steam](https://www.pcworld.idg.com.au/article/663924/valve-intriguing-steam-labs-experiments-help-find-new-games-play-sea-releases/), che ha fatto salire del 75% le visite a giochi "unici" ([PC Games Insider](https://www.pcgamesinsider.biz/news/69674/valve-has-updated-how-discovery-works-on-steam/)). I siti di "hidden gems" usano proprio la regola "voto alto + poche recensioni" ([European Gaming](https://europeangaming.eu/portal/latest-news/2020/11/23/82115/study-reveals-the-most-underrated-shows-films-games-youve-never-heard-of/)).
- **Tecnica**: una formula nel backend (un servizio `DiscoveryScoreService`) e un badge "💎 Gemma" sulle card. Lavora sopra la ricerca avanzata obbligatoria, quindi **migliora l'obbligatorio**.
- Costo: **basso**. Rapporto valore/costo altissimo. 🚀🚀🚀

### 🔥 4. "Hitster dei videogiochi": metti in ordine la storia
Mini-gioco ispirato a Hitster (il gioco da tavolo musicale): ti arriva la copertina di un gioco e la devi **trascinare al punto giusto della tua linea del tempo**. Se sbagli l'anno, la carta cade. Si vince a 10 carte. Ci sono una modalità a tema (solo Nintendo, solo anni '90…) e il record personale.
- **Perché è diversa**: i giochi "indovina il gioco" dagli screenshot sono ovunque ([Gamedle, GuessThe.Game, High Score Day](https://velersoftware.itch.io/guess-it-daily)). Quello sull'**ordine cronologico** no.
- **Tecnica**: CDK drag&drop di Angular, dati da `released`, validazione della risposta sul server (non si può barare dal browser).
- Costo: basso-medio. 🚀🚀🚀🚀

### 🔥 5. Indice di difficoltà "Platino"
Nel dettaglio: **"Quanto è difficile finirlo al 100%?"**. Calcoliamo un indice dalle percentuali di sblocco degli achievement (`/games/{id}/achievements`, campo `percent`): l'obiettivo più raro, la mediana, quanti sono sotto l'1%. Si mostra con un grafico e un'etichetta (*"Passeggiata" → "Solo per masochisti"*).
- **Perché è diversa**: è un dato che c'è nell'API ma che quasi nessuno usa. È **un'elaborazione nostra**, proprio quello che la traccia chiede al backend ("elaborare i dati ricevuti", §V.1).
- Costo: basso. ⚠️ Da verificare quanti giochi hanno davvero gli achievement su RAWG. 🚀🚀🚀

### 🔥 6. "Il tuo anno in pixel": la macchina del tempo
Inserisci l'anno di nascita e ottieni la **linea del tempo della tua vita da giocatore**: i giochi migliori dell'anno in cui sei nato, quanti anni avevi quando sono usciti i capolavori (*"Avevi 8 anni quando è uscito Minecraft"*) e le console di ogni periodo. Alla fine c'è una card condivisibile.
- **Perché è diversa**: esistono solo liste statiche ferme al 2021 ([Popular Mechanics](https://www.videogamecanon.com/lists/popular-mechanics-2019-2020/)) o legate a una sola saga ([My Mario Timeline](https://www.mariowiki.com/My_Mario_Timeline)).
- **Tecnica**: `/games?dates=AAAA-01-01,AAAA-12-31&ordering=-rating` per ogni anno, con cache aggressiva perché i dati storici non cambiano.
- Costo: basso. Perfetta per aprire la presentazione: *"vediamo l'anno del docente"*. 🚀🚀🚀

### 🔥 7. Radar delle uscite + calendario `.ics`
Pagina "In arrivo" con il **termometro dell'hype**, preso da `added_by_status.toplay` (quante persone lo vogliono giocare), conto alla rovescia, e pulsante **"Aggiungi al mio calendario"**. Il backend genera un file `.ics` (anche un feed a cui abbonarsi) con le uscite dei giochi in wishlist, che si importa in Google Calendar o Outlook.
- **Perché è diversa**: porta l'app **fuori dal browser**, nella vita reale dell'utente. Il feed `.ics` è un endpoint REST "vero" da mostrare in Postman.
- Costo: basso-medio. 🚀🚀🚀

### 🔥 8. Tier list dei preferiti
Trascini i tuoi preferiti nelle fasce S / A / B / C / D, esporti l'immagine e condividi il link. Il formato delle tier list è nato proprio nel gaming e TierMaker fa circa 17 milioni di visite al mese ([fonte](https://aiidelist.com/blog/tiermaker-founder-viral-growth-case-study)).
- **Tecnica**: CDK drag&drop, salvataggio nel DB, export PNG lato client.
- Costo: basso-medio. Trasforma i "preferiti" (obbligatori) in qualcosa di memorabile. 🚀🚀🚀

### 🔥 9. UI "che respira": tema dal colore della copertina
La pagina di dettaglio prende i **colori dominanti della copertina** (sfondo, accenti, glow), come fa Spotify con gli album. Il passaggio card → dettaglio è animato con le **View Transitions** (`withViewTransitions()` nel router di Angular), e c'è una **command palette `Ctrl+K`** per cercare da qualsiasi pagina.
- **Perché conta**: non è un bonus, ma alza i **10 punti della UI**, che valgono 5 volte i bonus. Non ho trovato nessun sito che cerchi giochi per colore o atmosfera: potrebbe diventare un'estensione futura (filtro "atmosfera: scura / colorata / neon").
- Costo: basso. 🚀🚀🚀🚀
- ✅ Verificato il 20/10: la risposta della libreria utente di RAWG contiene già i campi **`dominant_color`** e **`saturated_color`** (probabilmente anche `/games`, da confermare con la chiave), quindi il colore non va nemmeno calcolato.

### 🔥 10. "Game DNA" (aggiornata)
La trovate già come idea E al §5. Conferma dal mercato: Google Play ha lanciato a ottobre 2026 un "Game Finder" basato proprio sul "**Gamer DNA**" dell'utente ([mobilegamer.biz](https://mobilegamer.biz/google-play-is-adding-a-raft-of-new-discovery-curation-and-ai-features/), [Google](https://blog.google/products-and-platforms/platforms/google-play/google-play-updates-october-2026/)). È un'idea **di attualità**, da citare nella presentazione. Si alimenta dagli swipe dell'idea 1.

### Concept consigliato: "l'esploratore che ti conosce"

Non 10 feature scollegate, ma un filo unico da raccontare nella presentazione:

| Momento | Feature | Effetto |
|---|---|---|
| **Scopri** | Obbligatorio + **Gemme nascoste (3)** + **UI dal colore della copertina (9)** | La base è già diversa da quella di tutti gli altri |
| **Fatti conoscere** | **Game Match (1)** → Game DNA (10) | L'app impara i tuoi gusti e spiega perché ti consiglia un gioco |
| **Gioca con gli altri** | **Stasera si gioca (2)** oppure **Hitster (4)** | Momento "wow" in cui il docente partecipa |

Priorità: **3 e 9** (costano poco, migliorano direttamente obbligatorio e UI) → **1** → **4** oppure **2** (se il gruppo regge SignalR) → le altre come rifinitura. 5, 6 e 7 sono "piccole perle" da un paio di giorni l'una, da aggiungere se avanza tempo.

---

## 6. Piano a 4 settimane

| Settimana | Obiettivo |
|---|---|
| **1** | Setup del repo (GitHub, branch protection, template di PR, issue/board), solution .NET + progetto Angular, CI. Prendere la chiave RAWG. `RawgClient` con cache, endpoint `/api/games`, `/api/games/{id}`, `/api/genres`, `/api/platforms` **testati in Postman**. Design della UI (wireframe, palette, componenti) |
| **2** | Tutti gli obbligatori: home, ricerca, filtri, elenco, dettaglio, preferiti su DB, stati, responsive. **A fine settimana la checklist §3 deve essere tutta spuntata** |
| **3** | Bonus 1-5 della traccia + C/D. Proporre A/B al docente all'inizio della settimana |
| **4** | **Niente feature nuove.** Rifinitura della UI, test, bug, README, diagramma, Collection Postman definitiva, slide, **prove della presentazione** (ognuno spiega anche il codice degli altri) |

### Divisione nel gruppo (se siamo in 3-4)
Ognuno deve conoscere tutto, ma serve un "proprietario" per area:
- **Backend RAWG**: client, cache, endpoint dei giochi, Postman
- **Backend utente**: DB, EF Core, Identity/JWT, preferiti, wishlist, statistiche
- **Frontend core**: layout, routing, home, ricerca, elenco, dettaglio, stati
- **Frontend UX**: design system, responsive, dark mode, confronto, grafici, bonus

Il codice entra solo con PR revisionata da un altro membro: aiuta la qualità, la storia Git e il fatto che tutti conoscano tutto.

---

## 7. Parcheggio (solo se avanziamo tempo)

Dalla prima bozza, tenute per dopo: **Game Galaxy** (mappa di similarità), **Gamedle** (gioco quotidiano), **ricerca in linguaggio naturale con AI**, **Party Mode multiplayer** (SignalR). Valgono pochi punti rispetto al costo e andrebbero concordate col docente.

---

## 8. Decisioni aperte

- [ ] Quanti siamo nel gruppo e chi fa cosa
- [ ] DB: SQL Server Express (consigliato) o SQLite (più semplice da avviare)?
- [ ] Preferiti senza login: localStorage e migrazione al DB al login, oppure login obbligatorio per salvare?
- [ ] Quali bonus nostri proporre (consiglio: A, poi B)
- [ ] Nome e identità visiva. Idee: *Respawn*, *Checkpoint*, *Pixel Atlas*, *NextLevel*
- [ ] Versione di Angular: verificare la stable attuale su angular.dev

## 9. Domande per il docente

1. Possiamo proporre i bonus nostri (Game Match, Gemme nascoste, Hitster, A/B)? Valgono come le voci dell'elenco?
2. I +2 di bonus vanno divisi tra le feature, o basta una fatta bene?
3. Serve un deploy online o basta la demo in locale?
4. La documentazione tecnica ha un formato richiesto (PDF, README, wiki)?
5. Possiamo usare endpoint RAWG diversi da quelli elencati (store, DLC, serie, sviluppatori)?

## 10. Rischi noti su RAWG

- **Chiave obbligatoria** su ogni richiesta, altrimenti RAWG può bannare. La teniamo in `user-secrets`/variabili d'ambiente, **mai su Git**.
- **Limite mensile del piano gratuito** (fonti terze parlano di circa 20.000 richieste al mese: va verificato sul pannello della chiave). Si gestisce con la cache lato server. In più, con molti sviluppatori che provano in parallelo, conviene **una chiave per membro** in sviluppo.
- **Termini d'uso**: attribuzione con link attivo su ogni pagina, divieto di fare un "clone di RAWG". Un progetto didattico con valore aggiunto proprio va bene.
- Dati sporchi: giochi senza immagine, senza Metacritic, `released` null, descrizioni in HTML.

---

## Note e log delle decisioni
- *2026-10-16*: prima bozza delle idee, fatta prima di conoscere la traccia.
- *2026-10-20*: letta la traccia (riassunta in `SUBJECT.md`). Strategia riorientata: prima l'obbligatorio, la qualità e la UI (30/40 punti), poi i 5 bonus della traccia, al massimo 1-2 idee nostre. Le idee spettacolari passano nel "parcheggio".
- *2026-10-20*: aggiunte 10 idee innovative (§5-bis), costruite sui campi RAWG verificati sullo schema ufficiale. Concept proposto: "l'esploratore che ti conosce" (Gemme nascoste + tema dal colore della copertina + Game Match + un momento multiplayer).
