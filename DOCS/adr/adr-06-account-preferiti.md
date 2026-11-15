# ADR-06 — Preferiti, wishlist e profilo

- **Stato:** Accettato
- **Data:** 01/11/2026. Aggiornato l'08/11/2026 (passaggio a IGDB) e il 12/11/2026 (salviamo solo gli id)
- **Decisori:** Elmond0, K0ff33Blu3, fmartini02

## Contesto e problema

I preferiti sono obbligatori nella traccia; la wishlist separata e il profilo sono bonus. Ogni utente collegato deve poter salvare un gioco, toglierlo e ritrovare le sue liste anche da un altro dispositivo, quindi nel database e non nel `localStorage`.

Le decisioni da prendere erano:

1. quali endpoint espone la Web API;
2. che cosa salviamo di ogni gioco e da dove arrivano nome e copertina delle card;
3. come si comporta il pulsante quando l'utente non è collegato;
4. che cosa contiene il profilo.

## Opzioni considerate

**Che cosa salviamo di ogni gioco**

- **A. Una cache su database di tutti i giochi visti** (`game_cache`): memoria, poi database se la copia è fresca, poi IGDB, poi la copia scaduta se IGDB non risponde.
- **B. Una copia dei soli giochi salvati** (`game_snapshots`), scritta quando un utente salva e letta dalle liste.
- **C. Solo l'id del gioco**: le card delle liste si chiedono a IGDB con una sola richiesta e restano nella cache in memoria.

## Decisione

**C. Salviamo solo l'id del gioco.** Ci siamo arrivati dopo aver provato le altre due.

- La cache su database (A) è stata la prima scelta. Con IGDB la cache in memoria (`HybridCache`) basta per la scheda di un gioco, e il percorso a quattro livelli complicava il backend senza un vantaggio che si vedesse.
- La copia dei giochi salvati (B) l'abbiamo usata dal 1° al 12 novembre. Funzionava, ma ogni salvataggio doveva prima scaricare il gioco e riscriverne la copia, la copia invecchiava (se IGDB cambiava una copertina, la card restava quella vecchia) e passando da RAWG a IGDB tutte le copie andavano rifatte.
- Con il solo id (C) la Web API chiede a IGDB tutte le card di una lista con una richiesta (`where id = (…)`) e la tiene in memoria per 10 minuti. Le card sono sempre aggiornate e il database resta piccolo.

**Endpoint.** Tutti sotto `/api/me`, tutti solo per utenti collegati. L'utente si legge sempre dal cookie, mai da un parametro: così non si possono vedere o cambiare le liste di un altro.

| Richiesta | Risposta |
|---|---|
| `GET /api/me/favorites` | le card dei preferiti, dal più recente; accetta `platformIds` e `ordering` (`added`, `name`, `released`, `rating`, con `-` per l'ordine inverso) |
| `PUT /api/me/favorites/{gameId}` | `204`; `404` se il gioco non esiste su IGDB |
| `DELETE /api/me/favorites/{gameId}` | `204`, anche se il gioco non era tra i preferiti |
| `GET`, `PUT`, `DELETE /api/me/wishlist…` | come i preferiti |
| `GET /api/me/profile`, `PUT /api/me/profile` | email, nickname e avatar |
| `DELETE /api/me` | cancella l'account con le sue liste |

Per aggiungere usiamo `PUT` e non `POST` perché salvare due volte lo stesso gioco deve lasciare una riga sola. Nel database è un `INSERT … ON CONFLICT DO NOTHING`: anche un doppio click che manda due richieste nello stesso istante risponde `204` due volte, senza errori.

**Utente non collegato.** I pulsanti "Preferiti" e "Wishlist" nella scheda si vedono comunque; il click porta alla pagina di accesso. Così chi non ha un account scopre che la funzione esiste.

**Profilo.** Nickname (da 3 a 30 caratteri: lettere, numeri e `_`, unico) e un avatar scelto da un elenco fisso di immagini nostre. Non carichiamo immagini degli utenti: niente spazio su disco da gestire e niente contenuti da controllare.

### Conseguenze

- **Web API:** gli endpoint sono minimal API in `Data/MeEndpoints.cs`. Prima di salvare un gioco la Web API controlla che esista su IGDB, così nel database non finiscono id inventati.
- **Client:** `SaveButtons` nella scheda del gioco; la pagina `/profile` mostra le due liste e il form del profilo.
- **IGDB giù:** se IGDB non risponde e le card non sono già in memoria, le liste rispondono `502` come il resto del sito (gestione centralizzata degli errori). Le righe nel database non si perdono: tornano visibili appena IGDB risponde.
- **Cancellazione:** `DELETE /api/me` cancella l'utente; preferiti e wishlist spariscono con lui (`ON DELETE CASCADE`).
- **Limite noto:** per sapere se il gioco aperto è già salvato, `SaveButtons` scarica le due liste intere. Con liste lunghe sarebbe meglio un endpoint che dice solo sì o no per un gioco.

## Fonti

- [IGDB API](https://api-docs.igdb.com/) · [HybridCache in ASP.NET Core](https://learn.microsoft.com/aspnet/core/performance/caching/hybrid) · [PostgreSQL: INSERT … ON CONFLICT](https://www.postgresql.org/docs/current/sql-insert.html#SQL-ON-CONFLICT) · [Minimal API: gruppi di endpoint](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/route-handlers#route-groups) · [RFC 9110: metodi idempotenti](https://www.rfc-editor.org/rfc/rfc9110#name-idempotent-methods)
