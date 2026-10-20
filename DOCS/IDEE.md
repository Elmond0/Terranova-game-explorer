# Gaming Explorer: idee e strategia


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
