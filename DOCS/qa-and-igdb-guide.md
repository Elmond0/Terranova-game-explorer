# Domande e Risposte (4 Novembre 2026) e Guida IGDB

Trascrizione di due PDF:

- [Domande e Risposte 4 Novembre 2026](#domande-e-risposte-4-novembre-2026)
- [Guida per integrazione con IGDB](#guida-per-integrazione-con-igdb)

---

# Domande e Risposte 4 Novembre 2026

Il documento raccoglie chiarimenti e indicazioni per la realizzazione ed il rilascio della app Gaming Explorer, emersi durante la sessione di confronto con gli studenti di mercoledì 4 Novembre 2026.

## Sommario

1. [Cosa si intende esattamente con indicazione del caricamento dei dati?](#1-cosa-si-intende-esattamente-con-indicazione-del-caricamento-dei-dati)
2. [Il filtro per la valutazione deve basarsi sul rating o sul Metacritic score?](#2-il-filtro-per-la-valutazione-deve-basarsi-sul-rating-o-sul-metacritic-score)
3. [Come strutturare un frontend scalabile nel tempo?](#3-come-strutturare-un-frontend-scalabile-nel-tempo)
4. [Best practice per ottimizzare la UI per l'utente?](#4-best-practice-per-ottimizzare-la-ui-per-lutente)
5. [Conta di più UX o software mantenibile?](#5-conta-di-più-ux-o-software-mantenibile)
6. [Tutti i servizi devono avere CRUD? Friends, users, games ecc.?](#6-tutti-i-servizi-devono-avere-crud-friends-users-games-ecc)
7. [Quali criteri usare per scrivere codice più chiaro?](#7-quali-criteri-usare-per-scrivere-codice-più-chiaro)
8. [Dobbiamo consegnare anche .env? La repo deve funzionare appena clonata?](#8-dobbiamo-consegnare-anche-env-la-repo-deve-funzionare-appena-clonata)
9. [Preferenza di lingua per commenti e documentazione?](#9-preferenza-di-lingua-per-commenti-e-documentazione)
10. [L'uso di Postman è obbligatorio?](#10-luso-di-postman-è-obbligatorio)
11. [Occorre documentare i test effettuati con le API?](#11-occorre-documentare-i-test-effettuati-con-le-api)
12. [L'applicazione può girare in locale o deve essere pubblicata?](#12-lapplicazione-può-girare-in-locale-o-deve-essere-pubblicata)
13. [La gestione del GDPR è obbligatoria.](#13-la-gestione-del-gdpr-è-obbligatoria)

## 1. Cosa si intende esattamente con indicazione del caricamento dei dati?

Durante una chiamata asincrona l'utente deve capire che l'applicazione sta lavorando e che i dati non sono ancora disponibili. Ciò può accadere, per esempio, quando l'utente:

- effettua una ricerca
- applica un filtro
- apre il dettaglio di un gioco
- carica una lista di videogiochi

tra l'inizio della chiamata REST e la ricezione della risposta non dovrebbe trovarsi davanti a una schermata apparentemente bloccata o vuota.

È sufficiente mostrare un **loader, spinner, skeleton** oppure un messaggio "Caricamento...", disabilitando eventualmente le azioni che non devono essere eseguite mentre la richiesta è in corso.

L'applicazione deve comunicare chiaramente all'utente **cosa sta succedendo durante il caricamento dei dati**:

- **Loading** → i dati stanno arrivando
- **Successo** →
  - **Dati trovati → visualizzazione dei dati**
  - **Nessun risultato → messaggio informativo**
- **Errore** → la richiesta non è andata a buon fine e viene mostrato un messaggio
- **Nessun risultato** → la richiesta è andata a buon fine, ma non ci sono dati da mostrare

In sintesi: **l'utente non deve mai trovarsi davanti a una schermata vuota senza capire cosa sta succedendo.**

## 2. Il filtro per la valutazione deve basarsi sul rating o sul Metacritic score?

Differentemente da quanto indicato nel documento di progetto, il filtro si basa sul Metacritic score e non sul rating.

## 3. Come strutturare un frontend scalabile nel tempo?

Occorre valutare soprattutto **modularità e separazione delle responsabilità**, che sono principi esplicitamente richiesti dal progetto.

Con Angular, per esempio, una buona soluzione potrebbe separare:

- **pages/features**, per ricerca, dettaglio, preferiti ecc.
- **components**, per elementi UI riutilizzabili
- **services**, per l'accesso alle API/backend
- **models/interfaces**, per i modelli dati
- eventuali **guards/interceptors**, se implementano autenticazione
- componenti condivisi solo dove esiste effettivamente riutilizzo

**Regola pratica:**

Se domani aggiungessimo Wishlist o autenticazione, non dovremmo essere costretti a riscrivere metà dell'applicazione.

## 4. Best practice per ottimizzare la UI per l'utente?

**L'obiettivo principale è una UI semplice, intuitiva, responsive e coerente.**

Occorre concentrarsi soprattutto su:

1. navigazione semplice
2. feedback immediato alle azioni
3. loading/error/no-results chiaramente visibili
4. layout responsive
5. filtri comprensibili e facilmente modificabili
6. evitare informazioni o controlli inutili
7. coerenza grafica tra le diverse pagine

Non serve cercare una UI particolarmente sofisticata: è più importante che sia facile da usare.

L'applicazione deve avere un'interfaccia intuitiva, una navigazione semplice e tempi di risposta adeguati, oltre ad adattarsi a desktop, tablet e smartphone.

## 5. Conta di più UX o software mantenibile?

Riportiamo la griglia di valutazione:

| Criterio | Punteggio |
| --- | --- |
| Correttezza e completezza delle funzionalità obbligatorie | **10 punti** |
| Qualità del codice (organizzazione, modularità, leggibilità e buone pratiche) | **10 punti** |
| Interfaccia grafica, usabilità e responsive design | **10 punti** |
| Documentazione tecnica e repository Git | **5 punti** |
| Presentazione del progetto e capacità di illustrare le scelte progettuali | **5 punti** |
| Funzionalità Bonus | **+2 punti** |

La qualità e la manutenibilità del codice, così come la UI/UX hanno lo stesso peso nella valutazione complessiva: **a entrambe le aree sono assegnati 10 punti ciascuna**.

## 6. Tutti i servizi devono avere CRUD? Friends, users, games ecc.?

**No: non è necessario forzare un CRUD su ogni risorsa.**

Gli endpoint devono riflettere i **casi d'uso dell'applicazione**. Per esempio:

- videogiochi RAWG → prevalentemente **lettura/ricerca**
- preferiti → tipicamente **aggiunta, rimozione, elenco**
- utente → operazioni necessarie all'autenticazione/profilo, se implementato
- eventuali friendship → solo le operazioni necessarie alla feature

## 7. Quali criteri usare per scrivere codice più chiaro?

**Codice chiaro:**

- nomi comprensibili
- metodi piccoli e con una responsabilità chiara
- niente duplicazioni evidenti
- separazione tra UI, logica applicativa e accesso ai dati/API
- componenti e servizi riutilizzabili quando serve
- gestione centralizzata degli aspetti trasversali, quando ragionevole
- niente codice morto
- commenti utilizzati per spiegare il **perché**, non per tradurre banalmente il codice
- struttura consistente del progetto

Non bisogna cercare l'architettura perfetta, ma occorre essere in grado di rispondere in modo esaustivo a questa domanda: «**Perché questo codice è qui e non altrove?**»

Se la risposta è chiara e la struttura è coerente, siamo sulla giusta strada.

## 8. Dobbiamo consegnare anche .env? La repo deve funzionare appena clonata?

Il .env reale contenente secret/API key/password non deve essere committato.

Nel file **README** bisognerà spiegare:

1. quali prerequisiti installare
2. come creare/configurare l'ambiente
3. quali variabili valorizzare
4. come avviare frontend e backend
5. eventuali inizializzazioni/migrazioni del database

La presenza o meno di tutte o parte di queste informazioni sarà oggetto di valutazione.

Non pretendiamo che git clone sia sufficiente, senza alcuna configurazione, per poter utilizzare il progetto, ma chiediamo che il **repository contenga tutto ciò che serve per capire come configurare e avviare il progetto**, senza includere credenziali o secret.

Questo è sufficiente per effettuare la valutazione del Git repository.

## 9. Preferenza di lingua per commenti e documentazione?

Italiano o inglese: vanno entrambe bene, l'importante è essere coerenti.

**Esempio/suggerimento:**

- codice e naming → inglese
- commenti tecnici → preferibilmente inglese
- README/documentazione → italiano o inglese, purché venga utilizzata una lingua in maniera consistente

**Evitare qualcosa di misto del tipo:**

- `GetUser()`
- `SalvaPreferito()`
- `GameService`
- `GestioneUtentiController`

## 10. L'uso di Postman è obbligatorio?

**Postman** è una piattaforma completa a supporto dello sviluppo, testing, documentazione e gestione delle API. Non è obbligatorio usarla: potete usare qualsiasi altro strumento simile.

## 11. Occorre documentare i test effettuati con le API?

**No, non è richiesto.**

## 12. L'applicazione può girare in locale o deve essere pubblicata?

Ai fini della challenge non è richiesta la pubblicazione dell'applicazione: può girare anche solamente in locale.

## 13. La gestione del GDPR è obbligatoria.

**No.** Può rappresentare una funzionalità bonus, se correttamente gestito e documentato.

---

# Guida per integrazione con IGDB

Guida passo-passo per l'integrazione di IGDB.

## Passo 1: Registrazione dell'account e dell'App su Twitch (Console Sviluppatori)

Poiché IGDB fa parte dell'ecosistema Twitch/Amazon, l'accesso richiede un account sviluppatore collegato:

1. Accedere alla Console Sviluppatori Twitch effettuando il login con un account Twitch (è richiesta l'attivazione preventiva dell'autenticazione a due fattori - 2FA nelle impostazioni di sicurezza dell'account).
   - Se non si è in possesso di un account Twitch attivo con autenticazione a due fattori attivata, seguire i seguenti passaggi:
     - a) **Creazione di un nuovo account Twitch**:
       - I. Aprire il browser, collegarsi al sito ufficiale di Twitch e cliccare sul pulsante **Registrati** in alto a destra.
       - II. Inserire le informazioni richieste e completare la procedura di creazione dell'account.
     - b) **Attivazione dell'Autenticazione a Due Fattori (2FA)**:
       - I. Effettuare il login su Twitch con il nuovo account appena creato.
       - II. Cliccare sull'icona del proprio profilo in alto a destra e selezionare la voce **Impostazioni** dal menu a tendina.
       - III. Spostarsi nella scheda **Sicurezza e Privacy** della pagina delle impostazioni.
       - IV. Individuare la sezione Sicurezza > autenticazione in due fasi e avviare la configurazione.
       - V. Inserire il proprio numero di telefono cellulare quando richiesto e digitare il codice di verifica ricevuto tramite SMS per completare l'associazione e attivare la protezione 2FA sull'account.
2. Cliccare su **Registra la tua applicazione** (*Register Your Application*).
3. Compilare i campi richiesti:
   - **Nome:** Inserire un nome identificativo (es. *GamingExplorerBackend*).
   - **Reindirizzamento URL OAuth:** Inserire ad es. `https://localhost:5001/auth/callback` (va bene anche se è *fake*) e cliccare su **Aggiungi**.
   - **Categoria:** Selezionare *Website Integration*.
   - **Tipo di client:** Scegliere obbligatoriamente **Informazioni riservate** (*Confidential*), poiché le credenziali saranno gestite in modo sicuro sul backend.
4. Cliccare su **Crea**.
5. Dalla schermata di gestione della nuova app, annotare il **Client ID** e generare/copiare il **Client Secret** (nota: il secret viene mostrato una sola volta -> salvarlo in locale).

## Passo 2: Ottenimento del Token di Accesso (OAuth Token)

Prima di interrogare le API, il backend deve richiedere un token di autenticazione a Twitch tramite una chiamata HTTP POST:

- **URL:** <https://id.twitch.tv/oauth2/token>
- **Metodo:** POST
- **Body (x-www-form-urlencoded):**
  - `client_id`: [Inserire Client ID]
  - `client_secret`: [Inserire Client Secret]
  - `grant_type`: `client_credentials`
- **Risposta attesa:** Un oggetto JSON contenente l'`access_token` e la sua scadenza.

## Passo 3: Configurazione delle chiamate a IGDB (Postman e Backend)

A differenza di RAWG (che usava parametri URL), IGDB utilizza un endpoint radice comune (<https://api.igdb.com/v4/>) e accetta richieste POST strutturate con il linguaggio di query proprietario **Apicalypse** inviato nel Body in formato testo (*Text*).

Ogni richiesta inviata alle API di IGDB (`https://api.igdb.com/v4/...`) deve includere obbligatoriamente i seguenti **Headers**:

- `Client-ID`: [Il vostro Client ID di Twitch]
- `Authorization`: Bearer [Il token ottenuto al Passo 2]

## Tabella di Mappatura delle API (Da RAWG a IGDB)

Per facilitare l'adattamento del codice backend già scritto, ecco come convertire le principali chiamate richieste dalle specifiche del progetto:

| Funzionalità | Vecchia API RAWG (GET) Firenze 42 - Gaming Explorer.pdf | Nuova API IGDB (POST) | Sintassi Body (Apicalypse) |
| --- | --- | --- | --- |
| **Elenco / Ricerca giochi** | `/games` | <https://api.igdb.com/v4/games> | `fields name, rating, cover..url; search "Zelda"; limit 10;` |
| **Scheda di dettaglio** | `/games/{id}` | <https://api.igdb.com/v4/games> | `fields name, summary, artworks.*, platforms.name; where id = [ID];` |
| **Screenshot** | `/games/{id}/screenshots` | <https://api.igdb.com/v4/screenshots> | `fields url; where game = [ID];` |
| **Elenco generi** | `/genres` | <https://api.igdb.com/v4/genres> | `fields name, slug;` |
| **Elenco piattaforme** | `/platforms` | <https://api.igdb.com/v4/platforms> | `fields name, abbreviation;` |

**Nota importante: non esporre mai il Client Secret sul client frontend**
