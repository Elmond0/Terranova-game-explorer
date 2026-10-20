# API RAWG — elenco delle chiamate

Riferimento rapido per tutto il team, ricavato dalla specifica ufficiale ([api.rawg.io/docs](https://api.rawg.io/docs), formato Swagger 2.0: `https://api.rawg.io/docs/?format=openapi`). Consultato il 20/10/2026.

## Come si chiama l'API

| Voce | Valore |
|---|---|
| Indirizzo base | `https://api.rawg.io/api` |
| Metodo | solo `GET` (l'API è in sola lettura) |
| Chiave | parametro `key` in **ogni** richiesta: `?key=LA_TUA_CHIAVE`. Si ottiene su [rawg.io/apidocs](https://rawg.io/apidocs). Senza chiave le richieste possono essere bloccate |
| Formato risposta | JSON |

Esempio completo:
```
https://api.rawg.io/api/games?key=LA_TUA_CHIAVE&search=portal&page_size=10
```

> La chiave non va mai scritta nel codice né nel frontend: la usa solo il backend (user-secrets / variabile d'ambiente).

### Condizioni d'uso (dalla specifica)

- Gratuita per uso personale **a condizione di citare RAWG come fonte e mettere un link attivo a RAWG in ogni pagina che mostra i suoi dati**.
- Vietato usare l'API per creare un clone di RAWG.
- Il limite di richieste mensili del piano gratuito è indicato su [rawg.io/apidocs](https://rawg.io/apidocs): va verificato lì perché può cambiare.

### Paginazione

Tutte le chiamate che restituiscono un **elenco** hanno la stessa forma di risposta:

```json
{
  "count": 12345,
  "next": "https://api.rawg.io/api/games?page=2&...",
  "previous": null,
  "results": [ ... ]
}
```

| Parametro | Significato |
|---|---|
| `page` | numero di pagina (parte da 1) |
| `page_size` | quanti risultati per pagina |

`next` / `previous` sono gli URL della pagina successiva/precedente (`null` se non esiste).

### Identificativi

Dove il percorso contiene `{id}`, per i giochi si può usare sia l'**id numerico** (`3498`) sia lo **slug** (`grand-theft-auto-v`). `{game_pk}` è invece l'id numerico del gioco.

---

## 1. Giochi

| # | Chiamata | Cosa restituisce | Parametri (oltre a `key`) |
|---|---|---|---|
| 1 | `GET /games` | elenco di giochi, con ricerca e filtri | vedi tabella sotto |
| 2 | `GET /games/{id}` | dettaglio di un gioco | `id` (id o slug) |
| 3 | `GET /games/{game_pk}/additions` | DLC, edizioni GOTY e simili, app companion | `page`, `page_size` |
| 4 | `GET /games/{game_pk}/parent-games` | gioco "padre" di un DLC o di un'edizione | `page`, `page_size` |
| 5 | `GET /games/{game_pk}/game-series` | altri giochi della stessa serie | `page`, `page_size` |
| 6 | `GET /games/{game_pk}/development-team` | persone che hanno lavorato al gioco | `ordering`, `page`, `page_size` |
| 7 | `GET /games/{game_pk}/screenshots` | screenshot del gioco | `ordering`, `page`, `page_size` |
| 8 | `GET /games/{game_pk}/stores` | link ai negozi che vendono il gioco | `ordering`, `page`, `page_size` |
| 9 | `GET /games/{id}/achievements` | obiettivi (achievement) del gioco | `id` |
| 10 | `GET /games/{id}/movies` | trailer | `id` |
| 11 | `GET /games/{id}/reddit` | post recenti dal subreddit del gioco | `id` |
| 12 | `GET /games/{id}/suggested` | giochi visivamente simili — **solo piani business/enterprise** | `id` |
| 13 | `GET /games/{id}/twitch` | stream Twitch — **solo piani business/enterprise** | `id` |
| 14 | `GET /games/{id}/youtube` | video YouTube — **solo piani business/enterprise** | `id` |

Le chiamate 12–14 con la chiave gratuita **non sono disponibili**: non vanno usate nel progetto.

### Parametri di `GET /games`

| Parametro | Tipo | Significato | Esempio |
|---|---|---|---|
| `page`, `page_size` | intero | paginazione | `page=2&page_size=20` |
| `search` | testo | ricerca per nome | `search=portal` |
| `search_precise` | booleano | ricerca senza "tolleranza" agli errori di battitura | `true` |
| `search_exact` | booleano | ricerca esatta | `true` |
| `parent_platforms` | lista id | filtra per famiglia di piattaforme | `1,2,3` |
| `platforms` | lista id | filtra per piattaforma | `4,5` |
| `stores` | lista id | filtra per negozio | `5,6` |
| `developers` | lista id o slug | filtra per sviluppatore | `valve-software` |
| `publishers` | lista id o slug | filtra per editore | `electronic-arts` |
| `genres` | lista id o slug | filtra per genere | `action,indie` |
| `tags` | lista id o slug | filtra per tag | `singleplayer,multiplayer` |
| `creators` | lista id o slug | filtra per persona | `cris-velasco` |
| `dates` | intervallo di date | data di uscita (più intervalli separati da `.`) | `2010-01-01,2018-12-31` |
| `updated` | intervallo di date | data di aggiornamento su RAWG | `2020-12-01,2020-12-31` |
| `platforms_count` | intero | numero di piattaforme | `1` |
| `metacritic` | intervallo | voto Metacritic | `80,100` |
| `exclude_collection` | intero | esclude i giochi di una collezione | `123` |
| `exclude_additions` | booleano | esclude DLC ed edizioni | `true` |
| `exclude_parents` | booleano | esclude i giochi che hanno DLC | `true` |
| `exclude_game_series` | booleano | esclude i giochi che fanno parte di una serie | `true` |
| `exclude_stores` | lista id | esclude negozi | `5,6` |
| `ordering` | testo | ordinamento: `name`, `released`, `added`, `created`, `updated`, `rating`, `metacritic`; con `-` davanti l'ordine è decrescente | `-rating` |

### Campi principali della risposta

- **Elenco** (`/games`, ogni elemento di `results`): `id`, `slug`, `name`, `released`, `background_image`, `rating`, `rating_top`, `ratings_count`, `metacritic`, `playtime`, `updated`, `esrb_rating`, `platforms`, …
- **Dettaglio** (`/games/{id}`): tutti i campi dell'elenco più `description`, `name_original`, `website`, `background_image_additional`, `metacritic_url`, `alternative_names`, `reddit_url`, conteggi (`screenshots_count`, `achievements_count`, `additions_count`, …), …

> La specifica ufficiale non elenca tutti i campi realmente restituiti (per esempio `genres`, `developers`, `publishers` nel dettaglio). Prima di scrivere le classi C# conviene salvare una risposta vera con Postman e guardarla.

---

## 2. Cataloghi (generi, piattaforme, negozi, tag)

Utili per riempire i filtri della ricerca (menu a tendina).

| # | Chiamata | Cosa restituisce | Parametri (oltre a `key`) |
|---|---|---|---|
| 15 | `GET /genres` | elenco dei generi | `ordering`, `page`, `page_size` |
| 16 | `GET /genres/{id}` | dettaglio di un genere | `id` |
| 17 | `GET /platforms` | elenco delle piattaforme | `ordering`, `page`, `page_size` |
| 18 | `GET /platforms/lists/parents` | famiglie di piattaforme (es. PlayStation per PS2 e PS4) | `ordering`, `page`, `page_size` |
| 19 | `GET /platforms/{id}` | dettaglio di una piattaforma | `id` |
| 20 | `GET /stores` | elenco dei negozi digitali | `ordering`, `page`, `page_size` |
| 21 | `GET /stores/{id}` | dettaglio di un negozio | `id` |
| 22 | `GET /tags` | elenco dei tag | `page`, `page_size` |
| 23 | `GET /tags/{id}` | dettaglio di un tag | `id` |

## 3. Aziende e persone

| # | Chiamata | Cosa restituisce | Parametri (oltre a `key`) |
|---|---|---|---|
| 24 | `GET /developers` | elenco degli sviluppatori | `page`, `page_size` |
| 25 | `GET /developers/{id}` | dettaglio di uno sviluppatore | `id` |
| 26 | `GET /publishers` | elenco degli editori | `page`, `page_size` |
| 27 | `GET /publishers/{id}` | dettaglio di un editore | `id` |
| 28 | `GET /creators` | elenco delle persone (game designer, compositori, …) | `page`, `page_size` |
| 29 | `GET /creators/{id}` | dettaglio di una persona | `id` |
| 30 | `GET /creator-roles` | elenco dei ruoli (mestieri) delle persone | `page`, `page_size` |

---

## Riepilogo

30 chiamate, tutte `GET`, di cui 27 utilizzabili con la chiave gratuita. Per il progetto le più rilevanti sono:

| Bisogno | Chiamata |
|---|---|
| Ricerca e lista giochi | `GET /games?search=...` (+ filtri, `ordering`, paginazione) |
| Pagina di dettaglio | `GET /games/{id}` (+ eventualmente `/screenshots`, `/stores`, `/movies`) |
| Valori per i filtri | `GET /genres`, `GET /platforms`, `GET /stores`, `GET /tags` |

Ogni chiamata a RAWG consuma quota: la cache (issue #2 in memoria, issue #8 su PostgreSQL) serve proprio a non ripetere la stessa richiesta.
