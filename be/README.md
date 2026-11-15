# Web API

ASP.NET Core (.NET 10) Web API at `http://localhost:5290`. It is the only part of the app that talks to the database and to IGDB: the Blazor client never sees IGDB, its credentials or its JSON. The models exchanged with the client live in [`shared/GameExplorer.Shared`](../shared/GameExplorer.Shared).

In Development the OpenAPI document is at `/openapi/v1.json`; `GET /health` answers `Healthy` when the API is running (it does not depend on IGDB: if IGDB is down the API still works and answers `502` only on the game endpoints).

- [How a request flows](#how-a-request-flows)
- [Folders](#folders)
- [Running and configuration](#running-and-configuration)
- [What happens at startup](#what-happens-at-startup)
- [Endpoints](#endpoints)
- [How the game lists are built](#how-the-game-lists-are-built)
- [Search](#search)
- [From IGDB JSON to shared models](#from-igdb-json-to-shared-models)
- [Talking to IGDB: token, retries, rate limit](#talking-to-igdb-token-retries-rate-limit)
- [Cache](#cache)
- [Errors](#errors)
- [Account and sessions](#account-and-sessions)
- [Favorites, wishlist and profile](#favorites-wishlist-and-profile)
- [Database](#database)
- [Tests](#tests)
- [Troubleshooting](#troubleshooting)
- [Known limitations](#known-limitations)

## How a request flows

```
Blazor page ─► GameApiClient (fe) ──HTTP──► Controller ─► GameService ─► HybridCache ──miss──► IgdbClient
                                                                                                   │
              IgdbAuthHandler (Client-ID + Bearer token) ◄─────────────────────────────────────────┘
                     │
              Standard resilience handler (1 retry, 5 s per attempt, 12 s in total)
                     │
              IgdbRateLimitHandler (max 4 requests/s for the whole app)
                     │
                     ▼
              POST https://api.igdb.com/v4/<endpoint>   (Apicalypse query in the body)
```

Controllers are thin: they validate the input and call `GameService`. `GameService` builds the Apicalypse query, asks the cache, and only on a miss calls `IgdbClient`. The raw IGDB answer is converted to the shared models in `Services/IgdbDtos/IgdbMapper.cs` before it is cached, so the cache and the responses only ever contain `Game`, `GameDetails`, `Studio`, `Genre`, `Platform`, `PagedResult<T>`.

Account, favorites, wishlist and profile are minimal-API endpoints (`Auth/`, `Data/MeEndpoints.cs`) that use EF Core on PostgreSQL; favorites and wishlist also call IGDB for the game cards.

Why this split (Web API + Blazor WebAssembly, shared models): [ADR-01](../DOCS/adr/adr-01-stack.md), [ADR-02](../DOCS/adr/adr-02-contratto-api.md), [ADR-03](../DOCS/adr/adr-03-organizzazione-codice.md).

## Folders

| Path (in `GameExplorer.Api/`) | What it contains |
|---|---|
| `Program.cs` | startup: services, CORS, cookie, rate limits, IGDB HTTP pipeline, cache warm-up, middleware order |
| `Controllers/GamesController.cs` | search, game details, paged top-rated / recent / upcoming |
| `Controllers/HomeController.cs` | the three short lists of the home page |
| `Controllers/LookupControllers.cs` | genres, platforms, platform highlights |
| `Controllers/StudioController.cs` | studio details, upcoming games, released games |
| `Services/GameService.cs` | builds the queries, reads/writes the cache, maps the results |
| `Services/IgdbQueryFields.cs` | `where`, `sort` and `limit/offset` of every list |
| `Services/IgdbClient.cs` | `POST` to IGDB, `/count`, turns every failure into `IgdbException` |
| `Services/IgdbAuthHandler.cs` | adds `Client-ID` and `Authorization: Bearer`, refreshes the token on `401` |
| `Services/TwitchTokenProvider.cs` | gets the app token from Twitch (client credentials) and caches it |
| `Services/IgdbRateLimiter.cs` | `IgdbRateLimitHandler`: shared 4 req/s limiter, logs queue and IGDB time |
| `Services/IgdbCache.cs` | cache durations and cache keys, in one place |
| `Services/IgdbOptions.cs` | `ClientId`, `ClientSecret` (required, validated at startup) |
| `Services/IgdbDtos/` | IGDB JSON shapes (`internal`, never leave the API) and `IgdbMapper` |
| `Errors/` | `IgdbException` and `IgdbExceptionHandler` (→ `502`) |
| `Auth/` | register, login, logout, password change, PBKDF2 hashing |
| `Data/` | `AppDbContext`, `User`, entities, EF configurations, migrations, `/api/me` endpoints |

Tests are in `GameExplorer.Api.Tests/`.

## Running and configuration

The quickest way is from the repository root with `make` (on Windows, from **Git Bash**: the Makefile uses bash syntax):

```bash
make          # starts PostgreSQL in Docker and applies the migrations
make dev      # starts API + client with the secrets as environment variables
make test
```

Full guide, and the commands to run without `make`: [db/README.md](../db/README.md).

### Settings

| Setting | Where it comes from | Notes |
|---|---|---|
| `ConnectionStrings:Default` | `make dev` builds it from `secrets/api_db_password.txt` | missing → the API stops at startup |
| `Igdb:ClientId` | `secrets/twitch_id.txt` → `Igdb__ClientId` | `[Required]` + `ValidateOnStart`: missing → the API stops |
| `Igdb:ClientSecret` | `secrets/twitch_key.txt` → `Igdb__ClientSecret` | same; never sent to the client |
| `Cors:FrontendOrigin` | `appsettings.Development.json` (`http://localhost:5100`) | in other environments set `Cors__FrontendOrigin` |

The Twitch app must be of type **Confidential**, otherwise it has no secret usable for the client-credentials flow.

**Environment variables win over user-secrets.** If you also keep the keys in `dotnet user-secrets`, running the API alone (IDE, `dotnet run`) uses those, while `make dev` uses the files in `secrets/`. If one works and the other does not, the files are wrong. Write them without a trailing newline, `\r` or BOM:

```bash
printf '%s' 'YOUR_CLIENT_ID'     > secrets/twitch_id.txt
printf '%s' 'YOUR_CLIENT_SECRET' > secrets/twitch_key.txt
wc -c secrets/twitch_id.txt secrets/twitch_key.txt   # 30 bytes each
```

## What happens at startup

1. Options and secrets are validated (`ValidateOnStart`): wrong configuration fails immediately, not at the first request.
2. **Warm-up.** Before accepting requests, `Program.cs` runs one query on the database (opens the EF connection pool) and fills the cache with the seven home lists in parallel: top-rated, recent, upcoming and the highlights of PlayStation (167), Xbox (169), Nintendo (130) and PC (6). This also gets the Twitch token and warms the IGDB HTTP pipeline and the JSON serializer, so the first visitor gets the home from memory. Startup takes about 1-2 s longer.
3. If IGDB is down during the warm-up the API starts anyway and logs a warning: the lists will load on the first request.

The platform ids of the warm-up are the same as in the client's `Home.razor`: if you change one list, change the other.

## Endpoints

Bodies and responses use the models of `GameExplorer.Shared`. Game, studio, genre and platform ids are **IGDB ids**. Every endpoint accepts the request's `CancellationToken`: if the client leaves the page, the IGDB call is cancelled too.

### Games (no login)

| Method | Path | Returns |
|---|---|---|
| GET | `/api/games` | `PagedResult<Game>`: search ([parameters](#search)) |
| GET | `/api/games/{id}` | `GameDetails`, `404` if IGDB has no such game |
| GET | `/api/games/top-rated?page=&pageSize=` | `PagedResult<Game>` |
| GET | `/api/games/recent?page=&pageSize=` | `PagedResult<Game>` |
| GET | `/api/games/upcoming?page=&pageSize=` | `PagedResult<Game>` |
| GET | `/api/home/top-rated` | `Game[]` (20), for the home |
| GET | `/api/home/recent` | `Game[]` (20), for the home |
| GET | `/api/home/upcoming` | `Game[]` (20), for the home |
| GET | `/api/home/{id}` | same as `/api/games/{id}` |

The `/api/home/*` lists are the short, stricter versions shown on the home page; `/api/games/top-rated|recent|upcoming` are the paged "see all" pages, with looser filters so they contain more games. Paged endpoints take `page` 1-500 and `pageSize` 1-40 (defaults 1 and 20); out of range → `400`.

### Lookups and studios (no login)

| Method | Path | Returns |
|---|---|---|
| GET | `/api/genres` | `Genre[]`, all of them, by name |
| GET | `/api/platforms` | `Platform[]`, main platforms first (see below) |
| GET | `/api/platforms/{id}/highlights` | `Game[]` (20): best rated on that platform in the last 2 years |
| GET | `/api/studios/{id}` | `Studio`, `404` if not found |
| GET | `/api/studios/{id}/upcoming` | `Game[]`: next 5 releases |
| GET | `/api/studios/{id}/games?page=&pageSize=` | `PagedResult<Game>`: released games, most voted first |

`/api/platforms` puts PC, PS5, Xbox Series, Switch, PS4 and Xbox One at the top in this order (`MainPlatformIds` in `GameService`), then all the others alphabetically. IGDB can sort on one field only, so the ordering is done in memory on the full list (at most 500 platforms).

### Account

| Method | Path | Login | Notes |
|---|---|---|---|
| POST | `/api/account/register` | no | body `{ email, password }` |
| POST | `/api/account/login` | no | body `{ email, password }`, sets the cookie |
| POST | `/api/account/logout` | yes | removes the cookie |
| GET | `/api/account/manage/info` | yes | `ProfileResponse` |
| POST | `/api/account/manage/info` | yes | body `{ oldPassword, newPassword }` |

The paths are the same as ASP.NET Identity's `MapIdentityApi`, used before we wrote our own endpoints, so the client did not change.

### The user's own data (login required)

The user always comes from the cookie, never from the URL: a user can only read and change their own data.

| Method | Path | Notes |
|---|---|---|
| GET | `/api/me/favorites?platformIds=&ordering=` | `Game[]` |
| PUT | `/api/me/favorites/{gameId}` | `204`; adding twice is not an error; `404` if the game does not exist on IGDB |
| DELETE | `/api/me/favorites/{gameId}` | `204`, also if it was not there |
| GET / PUT / DELETE | `/api/me/wishlist…` | same as favorites |
| GET | `/api/me/profile` | `ProfileResponse` |
| PUT | `/api/me/profile` | body `UpdateProfileRequest` |
| DELETE | `/api/me` | deletes the account, its lists, and logs out |

Ready-made requests: [`postman/collections/`](../postman/collections/) (`GameExplorer-Games-IGDB` has both direct IGDB calls and calls to this API; the RAWG collections are kept only as history). The file `GameExplorer.Api.http` works from the IDE too.

## How the game lists are built

Every list is one Apicalypse query: `fields …; where …; sort …; limit …; offset …;`. The `fields` part belongs to the DTO (`IgdbGame.Fields`, `IgdbGameDetails.Fields`, `IgdbStudio.Fields`), the rest comes from `IgdbQueryFields.cs`. All lists below except genres and platforms require `cover != null` and `version_parent = null`, so they contain no games without a cover and no alternative editions (Deluxe, GOTY, Complete…).

| List | Filter | Sort | Size |
|---|---|---|---|
| Home top-rated, `/games/top-rated` | `rating_count > 100` | `rating desc` | 20 / paged |
| Home recent | released (date ≤ now), `hypes > 10` | `first_release_date desc` | 20 |
| `/games/recent` | released, `hypes > 5` | `first_release_date desc` | paged |
| Home upcoming | date in the next 2 years, `hypes > 20` | `first_release_date asc` | 20 |
| `/games/upcoming` | date in the next 2 years, `hypes > 10` | `first_release_date asc` | paged |
| Platform highlights | `platforms = (id)`, released in the last 2 years, `rating_count > 20` | `rating desc` | 20 |
| Studio upcoming | `involved_companies.company = id`, not released yet | `first_release_date asc` | 5 |
| Studio games | `involved_companies.company = id`, released | `total_rating_count desc` | paged |
| Genres, platforms | none | `name` | 500 |

Why these filters:

- **`rating_count` / `hypes` thresholds.** Sorting by `rating` alone puts a game with one 100/100 vote above famous titles. A minimum number of votes (or of "hypes" for games not out yet) keeps the lists meaningful.
- **`limit 500` on genres and platforms.** IGDB returns 10 results when `limit` is missing; 500 is its maximum.
- **`platforms = (id)` with round brackets** means "contains at least this one". Square brackets mean "contains all of them" (see [Search](#search)).

Paged lists run two IGDB calls in parallel: the page itself and `POST /games/count` with the same `where`, which gives `PagedResult.Total`.

## Search

`GET /api/games` binds the query string to `SearchFilter`; lists are repeated keys (`genreIds=4&genreIds=5`). With `[ApiController]` the `[Range]` attributes and `SearchFilter.Validate` run before the action: an invalid value gives `400` with the field in `errors`, without calling IGDB.

| Parameter | Meaning | Rule |
|---|---|---|
| `query` | text contained in the name or in an alternative name, case-insensitive | |
| `genreIds` | games that have **all** these genres | |
| `platformIds` | games released on **all** these platforms | |
| `developerIds` | games developed by these studios | |
| `fromYear`, `toYear` | first release year, both included | 1950-2100, `fromYear ≤ toYear` |
| `ordering` | `name`, `released`, `rating`, `added`, `created`, `updated`; `-` in front for descending | default: most voted first |
| `page`, `pageSize` | | 1-500, 1-40 (default 1, 20) |

How the query is built (`GameService.BuildSearchQuery`):

- **Text:** `name ~ *"text"* | alternative_names.name ~ *"text"*` in the `where`, instead of IGDB's `search` keyword. `search` orders only by name similarity and **cannot be combined with `sort`**, so results were full of DLC and unknown games with similar names. With a `where` we can sort ourselves.
- **Default order:** `total_rating_count desc`, so popular games come first, as on the big stores. IGDB sorts on **one field only** per query: `sort a desc, b asc` is not possible.
- **What is returned:** `game_type = (0,1,2,4,8,9,10,11)`: main games, DLC, expansions, standalone expansions, remakes, remasters, expanded games and ports. Bundles, packs, mods, episodes, seasons, forks and updates are excluded. `version_parent = null` removes the editions of the same game, which on IGDB are separate games with the same name and cover and would show up as duplicates.
- **Ordering values:** `released` → `first_release_date`, `rating` → `rating`, `created`/`added` → `created_at` (when the game was added to IGDB), `updated` → `updated_at`.
- **Dates:** years become Unix timestamps at midnight UTC: `≥ 1 Jan fromYear` and `< 1 Jan (toYear + 1)`.
- **Page limit:** `page > 500` is refused with `400` by the controller, before calling IGDB, so absurd offsets never reach it.

The cache key is the whole query (`search:<where>:<limit/offset>:<sort>`), so the same filters on the same page are served from memory for 10 minutes.

## From IGDB JSON to shared models

`IgdbMapper` is the only place that knows the IGDB format:

- **Dates:** IGDB sends Unix seconds; `FromUnix` turns them into `DateOnly?` using UTC (a local time zone could move the date by one day). A missing or `0` date becomes `null`, and the client shows "TBA".
- **Images:** IGDB sends `//images.igdb.com/…/t_thumb/<id>.jpg` (90×90). The mapper adds `https:` and swaps the size: `t_cover_big` for cards, `t_original` for the cover in the details page and for studio logos, `t_screenshot_big` for screenshots. `Width`/`Height` are those of the original image, not of the resized file.
- **Developers and publishers:** `involved_companies` is a list of links `{ company, developer, publisher }`, not of studios. `GameDetails.Developers` and `Publishers` are built by filtering those flags and mapping `company`, so they carry the studio's real id and name.
- **Website:** the official one (`websites` with `type == 1`).
- **Rating:** IGDB's `rating` (users), 0-100. Metacritic does not exist on IGDB (see [ADR-07](../DOCS/adr/adr-07-da-rawg-a-igdb.md)).
- **Nulls:** a game without cover, platforms or genres maps to `Image = null` and empty lists, never to an exception.

## Talking to IGDB: token, retries, rate limit

**Token.** IGDB wants two headers on every call: `Client-ID` and `Authorization: Bearer <token>`. `TwitchTokenProvider` gets the token with `POST https://id.twitch.tv/oauth2/token` (`grant_type=client_credentials`) and keeps it in `HybridCache` for one day (memory only, never in a distributed cache). `IgdbAuthHandler` adds the headers; if IGDB answers `401` it drops the cached token, gets a new one and retries **once**. Thanks to the cache's stampede protection, many parallel requests with no token cause one call to Twitch.

**Resilience** (`AddStandardResilienceHandler`, Polly):

| Setting | Value |
|---|---|
| Retries | 1 (2 attempts in total) |
| Timeout per attempt | 5 s |
| Total timeout, retries included | 12 s |
| `HttpClient.Timeout` | infinite: the two timeouts above do the job |

Only transient errors are retried: `408`, `429`, `5xx`, connection errors and attempt timeouts. A `400` (wrong query) or `404` is not retried.

**Rate limit.** IGDB accepts 4 requests per second. One `TokenBucketRateLimiter` (4 tokens per second, queue of 100, oldest first) is shared by the whole app, and `IgdbRateLimitHandler` waits for a token before every request. It is the innermost handler, so retries also go through it. If the queue is full it answers `429` locally, with `Retry-After`, without calling IGDB. The limiter is registered as a keyed service (`"igdb"`) so no other code gets it by mistake.

Every IGDB call is logged with the time spent in the queue and the time IGDB took:

```
IGDB /v4/games: queue 0 ms, igdb 412 ms, status 200
```

## Cache

`HybridCache` in memory (one API instance, so no Redis). Durations live in `IgdbCache`:

| What | Kept for | Keys |
|---|---|---|
| genres, platforms | 24 h | `genres`, `platforms` |
| game details, studio, studio games and upcoming | 6 h | `game:{id}`, `studio:{id}`, `studio:{id}:upcoming`, `studio:{id}:games:page=…:page-size=…` |
| home lists, paged top/recent/upcoming, platform highlights | 1 h | `home:top-rated`, `top-rated:page=…:page-size=…`, `platform:{id}`, … |
| searches, favorites and wishlist cards | 10 min | `search:<query>`, `cards:<sorted ids>` |
| Twitch token | 1 day | `igdb:twitch-token` |

- **Stampede protection:** if 50 requests ask for the same missing key, IGDB is called once and the others wait for the same result.
- **Errors are not cached:** an `IgdbException` leaves the cache empty, so the next request tries IGDB again.
- **Keys include everything that changes the answer** (page, page size, filters). A key without the page would return page 1 for every page.
- A `404` game (`null`) **is** cached for 6 hours like any other answer.
- The cache is emptied when the API restarts: after changing a query, restart before checking the result.

## Errors

All errors are JSON [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457).

| Status | When |
|---|---|
| `400` | invalid parameters or body; `errors` names the field |
| `401` | not logged in, or wrong email/password (same answer for both) |
| `404` | game, studio or user not found |
| `409` | nickname already taken |
| `429` | more than 10 register, login or password change attempts per minute from the same IP |
| `502` | IGDB or Twitch did not answer, answered with an error, timed out, or sent JSON we cannot read |

How the `502` works: `IgdbClient` catches every failure of the call (HTTP status not 2xx, timeout, connection error, resilience rejection, broken JSON, Twitch token error) and throws `IgdbException`, keeping the original as inner exception. `IgdbExceptionHandler` logs the full exception at `Warning` and writes a fixed body (`"Igdb service not available."`): the exception message, the IGDB URL and the stack trace go **only to the log**, never to the client. A request cancelled by the client is not an error and is not turned into `502`.

When you see `502`, read the API log: the line `IGDB request failed: IGDB answered 400 on 'games'` tells you which endpoint failed and how. A `400` from IGDB almost always means a syntax error in the Apicalypse query.

## Account and sessions

- **Cookie** `terranova.auth`, persistent. CORS accepts credentials only from `Cors:FrontendOrigin`. Without login the API answers `401`/`403` instead of redirecting to a login page. Why cookies and not JWT: [ADR-04](../DOCS/adr/adr-04-login-cookie.md).
- **Email:** trimmed, max 256 characters, must be a bare address (`"Name <a@b.it>"` is refused). Emails are unique and compared case-insensitively (ICU collation in PostgreSQL).
- **Password:** 8-128 characters, at least one uppercase letter and one special character. Stored as PBKDF2-SHA256, 600 000 iterations, 16-byte random salt (`Auth/Passwords.cs`). Iterations and salt are saved in the hash, so the iterations can be raised later without breaking old hashes. Hashes are compared in constant time.
- **No user enumeration:** login answers the same `401` for an unknown email and a wrong password. When the email does not exist the API still hashes a dummy password, so the answer takes the same time.
- **Brute force:** `register`, `login` and password change share a fixed-window limit of 10 requests per minute per IP (`429`).
- **Logout everywhere:** every user has a `SessionStamp`. It is saved in the cookie and checked against the database on every request. A password change or an account delete changes it, so the other devices are logged out at once; the device that changed the password gets a new cookie.

## Favorites, wishlist and profile

- **Only ids are stored.** The tables hold `user_id`, `game_id` and `added_at`. Name, cover, rating and platforms come from IGDB, so they never go out of date. `GameId` has no foreign key because games live on IGDB; the primary key `(user_id, game_id)` stops duplicates.
- **Adding** first checks that the game exists on IGDB (through the details cache, usually already filled by the details page). The insert uses `INSERT … ON CONFLICT DO NOTHING`: adding twice gives the same result, without a check-then-insert race. The interpolated values become SQL parameters, so there is no injection.
- **Reading a list** loads the ids, then gets all the cards from IGDB with **one** query (`where id = (…); limit 500;`), cached under the sorted id list. Filtering by platform (games on **all** the given platforms) and sorting are done in memory, because the card data is not in our tables.
- **Ordering:** `added` (default `-added`, newest first), `name`, `released`, `rating`, with `-` for descending. Any other value → `400`. Games without a release date are always last when sorting by `released`; the id breaks ties, so the order is stable between calls.
- **Profile:** `DisplayName` 3-30 characters, letters, digits and `_`, unique ignoring case (taken → `409`). `Avatar` must be one of the fixed avatars in the shared `Avatars.All`. These rules are checked in the API, not only in the Blazor form.
- **Delete account:** one `DELETE` on the user; favorites and wishlist go away by cascade; the cookie is removed.

The `/api/me` group is mapped once with `RequireAuthorization()`.

## Database

PostgreSQL in Docker, EF Core with Npgsql and snake_case names (`UseSnakeCaseNamingConvention`). Tables: `users`, `favorites`, `wishlist`. Each table's rules are in `Data/Configurations/`. Sensitive data logging (query values) is enabled only in Development.

The API runs as `terranova_api`, a user that can only read and write rows; migrations run as the owner user with `make migrate` (they are not applied by the API at startup). Details and reasons: [ADR-05](../DOCS/adr/adr-05-database.md), [ADR-06](../DOCS/adr/adr-06-account-preferiti.md), [db/README.md](../db/README.md).

Migration history, in short: Identity tables → game cache table → renamed to snapshots → `rawg_id` renamed to `game_id` when moving to IGDB (old RAWG ids deleted) → favorites and wishlist → Metacritic removed and rating on 0-100 → profile fields → Identity replaced by our own `users` table → snapshots dropped (cards now come from IGDB).

## Tests

```bash
make test        # or: dotnet test be/GameExplorer.Api.Tests
```

`ApiFactory` starts the real API in memory (`WebApplicationFactory<Program>`) with fake settings, and replaces only the **primary HTTP handlers** of IGDB and Twitch with `FakeIgdbHandler`. Auth handler, resilience pipeline, rate limiter, cache and error handler are the real ones, so the tests check the whole chain without network and without keys. The fake counts the calls it receives, which lets the tests check the cache.

| Suite | What it checks |
|---|---|
| `IgdbErrorTests` | `502` when IGDB answers an error, does not answer, times out, sends broken JSON or a non-array, or Twitch fails; `404` for a missing game; `200` with an empty list when IGDB finds nothing |
| `ResilienceTests` | retry on transient errors and on connection errors, gives up after the maximum, no retry on `4xx`, attempt timeout triggers a retry, token refreshed once on `401`, token requested once and reused |
| `CacheTests` | second identical request does not call IGDB, different id or filters do, errors and broken JSON are not cached, the cache recovers when IGDB recovers |
| `CorsTests` | preflight allowed from the frontend origin, refused from any other |
| `HealthTests` | `/health` is `200` also when IGDB is down |
| `IgdbConfigTests` | the API does not start without `ClientId` or `ClientSecret` |

`Data/MeEndpointsTests.cs` (favorites and wishlist against a real PostgreSQL through `PostgresFixture`) is commented out for now; when it is enabled, Docker must be running.

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| API works from the IDE but not with `make dev`; log shows `POST https://id.twitch.tv/oauth2/token` → `400` | the files in `secrets/` are wrong (swapped, other values, `\r` at the end). See [Settings](#settings) |
| Twitch answers `403` | wrong Client Secret, or the Twitch app is not Confidential |
| `502` and the log says `IGDB answered 400 on 'games'` | Apicalypse syntax error in a query (missing `;`, `&` before a clause…). Try the same body in Postman: IGDB's answer names the problem |
| An endpoint returns `[]` but IGDB has the games | the route placeholder and the parameter name differ (`{platformId}` vs `int id`), so the id is `0`; or a filter is too strict |
| A fix does not show up | the old answer is still in the cache (up to 24 h): restart the API |
| The same game appears twice in a search | an edition not filtered out (check `version_parent = null`), a port with the same name (`game_type` 11), or a tie in the sort field moving a game between two pages |
| First page load is slow | without warm-up the first request pays token + IGDB + JSON start-up; in Debug the Blazor client also downloads ~35 MB before calling the API (`dotnet publish -c Release` reduces it) |
| `429` without IGDB being called | the local rate limiter queue (100) is full: too many IGDB calls at once |

## Known limitations

Things that work today but are worth knowing, or fixing:

- **The search text is not escaped.** A `"` in `query` breaks the Apicalypse string, IGDB answers `400` and the client gets `502`. It should be escaped (`\"`) or removed in `BuildSearchQuery`. `Query` also has no `[MaxLength]`, and the id lists have no size limit.
- **`publisherIds`** exists in `SearchFilter` but `BuildSearchQuery` ignores it.
- **`developerIds`** puts two separate conditions on `involved_companies` (`company = […]` and `developer = true`). IGDB may match them on different companies of the same game, so a game where the studio is only the publisher can appear.
- **`ordering=added`** sorts by the date the game was added to IGDB (same as `created`), not by popularity.
- **`PagedResult.Total`** is not capped: with more than 500 pages the "next" link of page 500 gets a `400`.
- **Ties and pagination:** many games share the same `total_rating_count` (often 0), and IGDB can return ties in a different order between two requests, so a game can show up on two pages.
- **Metacritic** is not available on IGDB; `aggregated_rating` (critics) is the closest value, still to be confirmed with the teachers ([ADR-07](../DOCS/adr/adr-07-da-rawg-a-igdb.md)).
- **`/api/home/{id}`** duplicates `/api/games/{id}`.
- The warm-up platform ids are duplicated in `Program.cs` and the client's `Home.razor`.
- The cookie's Data Protection keys are kept in memory: in a container with a read-only file system every restart logs everyone out (`AddDataProtection().PersistKeysToFileSystem(...)` would fix it).