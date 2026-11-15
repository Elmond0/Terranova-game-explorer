# Web API

ASP.NET Core (.NET 10) Web API at http://localhost:5290: the only part of the app that talks to the database and to IGDB. How to start it: [db/README.md](../db/README.md). The models exchanged with the client live in [`shared/GameExplorer.Shared`](../shared/GameExplorer.Shared).

In Development the OpenAPI document is at `/openapi/v1.json`; `GET /health` answers `Healthy` when the API is running.

- [Folders](#folders)
- [Configuration](#configuration)
- [Endpoints](#endpoints)
- [Search parameters](#search-parameters)
- [Errors](#errors)
- [Login and sessions](#login-and-sessions)
- [IGDB and cache](#igdb-and-cache)
- [Tests](#tests)

## Folders

| Folder (in `GameExplorer.Api/`) | What it contains |
|---|---|
| `Program.cs` | startup: services, CORS, cookie, rate limit, middleware order |
| `Controllers/` | IGDB endpoints: games, genres, platforms, studios |
| `Services/` | `GameService` (IGDB queries + cache), `IgdbClient`, Twitch token, cache durations (`IgdbCache`) |
| `Services/IgdbDtos/` | IGDB JSON shapes and the mapper to the shared models |
| `Auth/` | register, login, logout, password change, password hashing |
| `Data/` | `AppDbContext`, entities, EF Core configurations and migrations, `/api/me` endpoints |
| `Errors/` | IGDB failure → `502` |

Tests are in `GameExplorer.Api.Tests/`.

## Configuration

The connection string and the two IGDB keys come from `secrets/` as environment variables: `make dev` sets them, without `make` see [B3](../db/README.md#b3-set-the-backend-variables). The API stops at startup if one is missing.

`Cors:FrontendOrigin` is in `appsettings.Development.json` (`http://localhost:5100`); in any other environment set `Cors__FrontendOrigin`.

## Endpoints

Bodies and responses use the models of `GameExplorer.Shared`. Game ids are IGDB ids.

Games and lookups (no login):

| Method | Path | Returns |
|---|---|---|
| GET | `/api/games` | paged search: `PagedResult<Game>` ([parameters](#search-parameters)) |
| GET | `/api/games/{id}` | `GameDetails` |
| GET | `/api/games/top-rated` | `Game[]`: best rated since 2020 |
| GET | `/api/games/recent` | `Game[]`: released in the last 30 days |
| GET | `/api/games/upcoming` | `Game[]`: coming in the next 12 months |
| GET | `/api/genres` | `Genre[]` |
| GET | `/api/platforms` | `Platform[]` |
| GET | `/api/platforms/{id}/highlights` | `Game[]`: best rated on that platform in the last 2 years |
| GET | `/api/studios/{id}` | `Studio` |
| GET | `/api/studios/{id}/upcoming` | `Game[]`: next 5 releases |
| GET | `/api/studios/{id}/games?page=&pageSize=` | `PagedResult<Game>`: released games, most rated first |

Account:

| Method | Path | Login | Notes |
|---|---|---|---|
| POST | `/api/account/register` | no | body `{ email, password }` |
| POST | `/api/account/login` | no | body `{ email, password }`, sets the cookie |
| POST | `/api/account/logout` | yes | removes the cookie |
| GET | `/api/account/manage/info` | yes | `ProfileResponse` |
| POST | `/api/account/manage/info` | yes | body `{ oldPassword, newPassword }` |

The user's own data (login required; the user comes from the cookie, never from the URL):

| Method | Path | Notes |
|---|---|---|
| GET | `/api/me/favorites?platformIds=&ordering=` | `Game[]` |
| PUT | `/api/me/favorites/{gameId}` | adding twice is not an error |
| DELETE | `/api/me/favorites/{gameId}` | `204` also if it was not there |
| GET / PUT / DELETE | `/api/me/wishlist...` | same as favorites |
| GET | `/api/me/profile` | `ProfileResponse` |
| PUT | `/api/me/profile` | body `UpdateProfileRequest` |
| DELETE | `/api/me` | deletes the account and logs out |

On the lists `ordering` is `added` (default `-added`), `name`, `released` or `rating`, with `-` for descending; `platformIds` can be repeated and keeps the games on all of them.

Ready-made requests: [`postman/collections/`](../postman/collections/), guide in [DOCS/test-postman.md](../DOCS/test-postman.md).

## Search parameters

`GET /api/games` takes the fields of `SearchFilter` as query string; lists are repeated keys (`genreIds=4&genreIds=5`).

| Parameter | Meaning | Rule |
|---|---|---|
| `query` | text in the name or in an alternative name | |
| `genreIds`, `platformIds` | games with all of them | |
| `developerIds` | developer studios | |
| `fromYear`, `toYear` | first release year, both included | 1950-2100 |
| `ordering` | `name`, `released`, `rating`, `added`, `created`, `updated`; `-` for descending | default: most rated first |
| `page`, `pageSize` | | 1-500, 1-40 (default 20) |

DLC, mods and bundles are never returned.

## Errors

All errors are JSON [ProblemDetails](https://www.rfc-editor.org/rfc/rfc9457).

| Status | When |
|---|---|
| `400` | invalid parameters or body; `errors` names the field |
| `401` | not logged in, or wrong email/password (same answer for both) |
| `404` | game, studio or user not found |
| `409` | nickname already taken |
| `429` | more than 10 register, login or password change attempts per minute from the same IP |
| `502` | IGDB did not answer or answered with an error |

## Login and sessions

- Cookie `terranova.auth`; CORS accepts it only from `Cors:FrontendOrigin`.
- Passwords: 8-128 characters, one uppercase letter and one special character; stored as PBKDF2-SHA256 hashes (`Auth/Passwords.cs`).
- A password change or an account delete ends the session on every other device at once (session stamp checked on each request).

Why cookies: [ADR-04](../DOCS/adr/adr-04-login-cookie.md).

## IGDB and cache

The API gets a token from Twitch and queries `https://api.igdb.com/v4/` (3 s per attempt, 2 retries). Answers stay in memory (`HybridCache`):

| What | Kept for |
|---|---|
| genres, platforms | 24 h |
| game details, studios | 6 h |
| home lists, platform highlights | 1 h |
| searches, favorites and wishlist cards | 10 min |

The database stores only game ids: the cards of favorites and wishlist come from IGDB with one query per list. Why IGDB: [ADR-07](../DOCS/adr/adr-07-da-rawg-a-igdb.md).

## Tests

`make test` ([db/README.md](../db/README.md#path-a-with-make)). IGDB and Twitch are faked, so no keys are needed; Docker must be running for the database tests.
