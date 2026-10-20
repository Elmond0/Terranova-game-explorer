# Gaming Explorer — 10 idee

> Dieci idee in più rispetto alle funzionalità obbligatorie, da sfogliare e scegliere insieme.
> Usano solo i dati di RAWG accessibili con la chiave gratuita e lo stack dell'[ADR-01](adr/adr-01-stack.md) (Blazor WebAssembly + MudBlazor, ASP.NET Core, PostgreSQL).
> Aggiornato il 20/10/2026.

| # | Idea | In una riga | Costo |
|---|---|---|---|
| 1 | [💎 Gemme nascoste](#1-gemme-nascoste) | uno slider sposta la ricerca dai giochi famosi ai capolavori sconosciuti | basso |
| 2 | [❤️ Game Match](#2-game-match) | swipe sulle card: l'app impara i tuoi gusti e spiega perché ti consiglia un gioco | medio |
| 3 | [🎉 Stasera si gioca](#3-stasera-si-gioca) | una stanza con gli amici, swipe sullo stesso mazzo, "It's a match!" | alto |
| 4 | [⏱️ Ho due ore](#4-ho-due-ore) | scegli quanto tempo hai e come ti senti, l'app propone tre giochi | basso-medio |
| 5 | [🕹️ Metti in ordine la storia](#5-metti-in-ordine-la-storia) | mini-gioco: trascina le copertine nell'anno giusto della linea del tempo | basso-medio |
| 6 | [📅 Accadde oggi](#6-accadde-oggi) | in Home i giochi che compiono gli anni oggi, e quelli usciti il giorno in cui sei nato | basso |
| 7 | [🏆 Indice Platino](#7-indice-platino) | quanto è difficile finire un gioco al 100%, calcolato dagli achievement | basso |
| 8 | [🔔 Radar delle uscite](#8-radar-delle-uscite) | le uscite della wishlist nel tuo Google Calendar, con l'hype della community | basso-medio |
| 9 | [🥇 Tier list](#9-tier-list) | i preferiti trascinati nelle fasce S-A-B-C-D, da condividere | basso-medio |
| 10 | [🎨 Il gioco colora la pagina](#10-il-gioco-colora-la-pagina) | il dettaglio prende i colori della copertina, con ricerca rapida `Ctrl+K` | basso |

---

## 1. Gemme nascoste

Uno slider **Mainstream ↔ Gemme** nella ricerca. A sinistra i giochi che conoscono tutti, a destra quelli **ottimi ma poco conosciuti**, con un badge 💎 sulle card.

- **Perché colpisce:** è il principio dell'*Interactive Recommender* di Steam, che ha fatto salire del 75% le visite a giochi poco noti ([PC Games Insider](https://www.pcgamesinsider.biz/news/69674/valve-has-updated-how-discovery-works-on-steam/), [PCWorld](https://www.pcworld.idg.com.au/article/663924/valve-intriguing-steam-labs-experiments-help-find-new-games-play-sea-releases/)).
- **Come:** il backend calcola un **indice gemma**: voto alto (`rating`), abbastanza voti perché il voto sia affidabile (`ratings_count`) e pochi utenti che lo hanno in libreria (`added`). È una formula di tre righe in un servizio C#, facile da spiegare.
- **Bonus per il progetto:** si costruisce sopra la ricerca avanzata obbligatoria, quindi la migliora invece di aggiungere una pagina in più.

## 2. Game Match

Le card arrivano una alla volta e si trascinano in tre zone: **❤️ Lo amo · 🤔 Forse · ✖️ Non fa per me**. Ogni scelta aggiorna il tuo **profilo di gusto** e la card successiva tiene conto di quel profilo. Ogni consiglio dice **perché**: *"Ti propongo Hades perché hai amato tre giochi roguelike con combattimento veloce"*.

- **Perché colpisce:** a ottobre 2026 Google Play ha lanciato il *Game Finder*, che consiglia giochi in base al "Gamer DNA" dell'utente e spiega il motivo di ogni suggerimento ([Android Authority](https://androidauthority.com/google-play-game-finder-gamer-dna-3721539), [mobilegamer.biz](https://mobilegamer.biz/google-play-is-adding-a-raft-of-new-discovery-curation-and-ai-features/)). È un'idea di attualità e si può citare in presentazione.
- **Come:** un *content-based filtering*: ogni tag e genere ha un peso, ❤️ lo aumenta e ✖️ lo diminuisce. Il punteggio di un gioco è la somma dei pesi dei suoi tag. Si spiega in due minuti alla lavagna. Il trascinamento si fa con `MudDropContainer` di MudBlazor.
- **Bonus per il progetto:** ❤️ aggiunge ai preferiti, 🤔 alla wishlist, e il profilo alimenta le statistiche personali. Riusa tre funzionalità che ci sono comunque.

## 3. Stasera si gioca

Si crea una **stanza** e si condivide il link (o il QR code). Ogni amico indica le console che ha e fa swipe sullo stesso mazzo. Il mazzo contiene solo giochi multiplayer o co-op disponibili **su piattaforme che tutti hanno**. Quando un gioco piace a tutti: **"It's a match!"**.

- **Perché colpisce:** risolve un problema vero, "a cosa giochiamo stasera?". In presentazione si fa partecipare il docente dal telefono.
- **Come:** **SignalR** di ASP.NET Core per mandare i voti in tempo reale a tutti i partecipanti. Versione semplice senza SignalR: il frontend chiede lo stato della stanza ogni 2 secondi. Il filtro usa `platforms` e i tag multiplayer/co-op di RAWG.
- **Attenzione:** è l'idea più costosa. Ha senso solo dopo la 2, di cui riusa le card.

## 4. Ho due ore

L'utente risponde a due domande: **quanto tempo ho** (una sera, un weekend, un mese) e **come mi sento** (rilassato, competitivo, avventuroso, malinconico…). L'app propone tre giochi, con un pulsante **"Un altro!"**.

- **Perché colpisce:** cercare per stato d'animo invece che per genere è una tendenza della game discovery, con siti dedicati come il *What Game Should I Play?* di Whisper of the House ([sito](https://www.whisperofthehouse.com/what-game-should-i-play)). Nessun catalogo classico lo combina con il tempo disponibile.
- **Come:** il backend traduce ogni stato d'animo in un gruppo di tag e generi RAWG (per esempio *rilassato* → `relaxing`, `casual`, `exploration`) e filtra per `playtime`, le ore medie di gioco. Stesso dato per un extra sulla wishlist: *"La tua wishlist vale 312 ore: a un'ora a sera la finisci a giugno"*. App come IGN Playlist e SavePoint fanno proprio questo calcolo sul backlog ([twoaveragegamers](https://www.twoaveragegamers.com/howlongtobeat-vs-savepoint-the-answer-is-both/)).
- **Bonus per il progetto:** è **logica nostra** nel backend, non un semplice inoltro a RAWG, che è esattamente quello che la traccia chiede.

## 5. Metti in ordine la storia

Mini-gioco ispirato a *Hitster*: arriva la copertina di un gioco e va **trascinata nel punto giusto della tua linea del tempo**. Se sbagli l'anno la carta cade, a 10 carte giuste hai vinto. Ci sono modalità a tema (solo Nintendo, solo anni '90…) e il record personale.

- **Perché colpisce:** i giochi "indovina il gioco dallo screenshot" sono ovunque; quello sull'**ordine cronologico** no. È il momento divertente della presentazione.
- **Come:** dati da `released`, trascinamento con `MudDropContainer`. La risposta la controlla il **server**, così non si bara dal browser. Il record si salva nel database.

## 6. Accadde oggi

Una sezione della Home: **"Oggi compiono gli anni"**, con i giochi usciti in questa data 10, 20, 25 e 30 anni fa (*"Oggi Half-Life 2 compie 22 anni"*). Nell'area personale c'è la variante **"Il giorno in cui sei nato"**: i giochi usciti quel giorno e i migliori di quell'anno, da condividere come card.

- **Perché colpisce:** la Home cambia ogni giorno, e chi visita ha un motivo per tornare. È un formato collaudato nella stampa di settore: *This Day in Gaming* di RPGFan ([RPGFan](https://classic.rpgfan.com/news/2018/8549.html)) e *This Day in Game History* di MobyGames, che gli utenti usano proprio per cercare i giochi usciti il giorno del loro compleanno ([Bulbagarden](https://bulbagarden.net/threads/video-games-released-on-your-birthday.302564/)).
- **Come:** `GET /games?dates=AAAA-MM-GG,AAAA-MM-GG&ordering=-added`, una chiamata per ogni anniversario. Il risultato resta valido fino a mezzanotte, quindi la cache su PostgreSQL lo serve a tutti con quattro chiamate a RAWG al giorno.
- **Bonus per il progetto:** arricchisce la Home, che è obbligatoria, e mostra bene a cosa serve la cache.

## 7. Indice Platino

Nel dettaglio di un gioco: **"Quanto è difficile finirlo al 100%?"**. Un'etichetta che va da *Passeggiata* a *Solo per masochisti*, con il grafico delle percentuali di sblocco degli achievement.

- **Perché colpisce:** il dato è nell'API ma quasi nessuno lo usa. Chi cerca il "platino" lo controlla sempre prima di comprare un gioco.
- **Come:** `GET /games/{id}/achievements` restituisce la percentuale di giocatori che ha sbloccato ogni achievement. Il backend calcola l'achievement più raro, la mediana e quanti sono sotto l'1%. Il grafico si fa con `MudChart`.
- **Da verificare:** con la chiave, quanti giochi hanno davvero gli achievement su RAWG.

## 8. Radar delle uscite

Una pagina **"In arrivo"** con il conto alla rovescia e il **termometro dell'hype**, cioè quante persone su RAWG hanno messo il gioco tra quelli da giocare. Il pulsante **"Aggiungi al calendario"** scarica un file `.ics` con le uscite dei giochi in wishlist, che si importa in Google Calendar o Outlook.

- **Perché colpisce:** porta l'app **fuori dal browser**, nel calendario dell'utente.
- **Come:** `GET /games?dates=oggi,fra-un-anno&ordering=-added`. L'hype viene da `added_by_status.toplay`. Il file `.ics` è testo semplice generato dal backend: un endpoint REST vero da mostrare in Postman.
- **Bonus per il progetto:** dà un senso concreto alla wishlist, che è un bonus della traccia.

## 9. Tier list

I preferiti si trascinano nelle fasce **S · A · B · C · D**. La classifica si salva e si condivide con un link.

- **Perché colpisce:** la tier list è nata nel gaming ed è uno dei formati più condivisi. TierMaker, il sito più usato, fa circa 17 milioni di visite al mese ([fonte](https://aiidelist.com/blog/tiermaker-founder-viral-growth-case-study)).
- **Come:** `MudDropContainer` con una zona per fascia, salvataggio su PostgreSQL (una riga per gioco con la sua fascia), pagina pubblica in sola lettura per il link condiviso.
- **Bonus per il progetto:** trasforma i preferiti obbligatori in qualcosa che si ricorda.

## 10. Il gioco colora la pagina

La pagina di dettaglio prende i **colori dominanti della copertina**: sfondo, accenti e bagliore, come fa Spotify con gli album. Da qualsiasi pagina, **`Ctrl+K`** apre una ricerca rapida che porta direttamente al gioco.

- **Perché colpisce:** è la prima cosa che si vede in demo. Non è un bonus, ma alza i **10 punti dell'interfaccia**, che valgono cinque volte i bonus.
- **Come:** il colore si applica cambiando a runtime il tema di MudBlazor (`MudThemeProvider`), così si adatta anche alla dark mode. La ricerca rapida è un `MudDialog` con un campo di ricerca collegato all'endpoint di ricerca che esiste già.
- **Da verificare:** RAWG potrebbe restituire già il colore dominante dell'immagine (campi `dominant_color` e `saturated_color`, non documentati nella specifica ufficiale). Se non c'è, lo calcola il backend dall'immagine.
