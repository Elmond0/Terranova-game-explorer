# Client

Blazor WebAssembly (.NET 10) with [MudBlazor](https://mudblazor.com/) 9, at http://localhost:5100. It runs in the browser and talks only to the [Web API](../be/README.md); how to start it: [db/README.md](../db/README.md). Why Blazor and MudBlazor: [ADR-01](../DOCS/adr/adr-01-stack.md).

The interface is in Italian, the code in English ([ADR-03](../DOCS/adr/adr-03-organizzazione-codice.md)).

- [Pages](#pages)
- [Folders](#folders)
- [Talking to the API](#talking-to-the-api)
- [Login](#login)
- [Theme](#theme)
- [Home rows](#home-rows)
- [Side menu](#side-menu)
- [Tests](#tests)

## Pages

| Path | Page |
|---|---|
| `/` | search bar, rows of games (most liked, recent, upcoming) and one row per platform family |
| `/search` | search with filters, sorting and pages; the filters live in the address (`?q=&genres=&platforms=&fromYear=&toYear=&ordering=&page=`), so a search can be shared or reloaded |
| `/games/{id}` | game page: cover, ratings, description, screenshot gallery, studios, favorite and wishlist buttons |
| `/games/top-rated`, `/games/recent`, `/games/upcoming` | "Vedi tutti" of a home row: the whole list in pages (`?page=`, at most 500, like the API); any other name shows "not found" |
| `/studios/{id}` | studio page: logo, its games with sorting, average rating |
| `/profile` | nickname, avatar, favorites and wishlist (login required) |
| `/account/login`, `/account/register` | login and registration; after login you go back to the page you came from |
| any other address | "page not found" (`NotFound.razor`, set as `NotFoundPage` in `App.razor`) |

## Folders

| Folder (in `GameExplorer.Client/`) | What it contains |
|---|---|
| `Program.cs` | startup: API address, real or fake client, login, MudBlazor |
| `Pages/` | one component per page |
| `Components/` | shared pieces: game card and row, skeleton while loading, error, not found, no results, save buttons, avatar |
| `Layout/` | navbar, side menu (opens on phones), footer with credits |
| `Services/` | API client, cookie handler, login state, login address with the way back (`NavigationExtensions`), "Cerca" from the menu (`SearchFocusService`), studio country names (`CountryNames`) |
| `wwwroot/` | `index.html`, styles, `appsettings.Development.json`, `js/carousel.js` |

Tests are in `GameExplorer.Client.Tests/`.

## Talking to the API

Pages never use `HttpClient` directly: they ask `IGameApiClient`, which has two versions.

| Version | When |
|---|---|
| `GameApiClient` | normal use: calls the Web API, 10 s timeout |
| `FakeGameApiClient` | tests, and working on pages without the API running; real IGDB images, data in memory |

Both are chosen in `wwwroot/appsettings.Development.json`:

```json
"Api": { "BaseUrl": "http://localhost:5290/", "UseFake": false }
```

What the pages do with the answers:

- `404` on a game or a studio → "not found" message; on a list → error, not an empty list.
- Any other failure → `ApiException` → error message with a retry button. A row of the home that fails does not stop the others.
- A search refused by the API (`400`) explains why instead of offering a retry.

The client is public, so it holds no keys: everything secret stays in the Web API.

## Login

- `CookieHandler` sends the login cookie with every request (`BrowserRequestCredentials.Include`): the API and the client are on different ports, and without it the browser would not send it.
- `CookieAuthenticationStateProvider` asks the API who is logged in (`api/account/manage/info`) and puts email, nickname and avatar in the login state used by the navbar and the pages.
- Password rules in the registration form are the same as the backend's, so most errors are shown before sending.

Avatars are icons from [game-icons.net](https://game-icons.net) (CC BY 3.0), credited in the footer.

## Theme

Light or dark, from the button in the navbar. The choice is saved in `localStorage`; with no choice the app follows the system theme. A small script in `index.html` sets it before Blazor starts, so the page never flashes white while loading.

## Home rows

The rows of the home scroll endlessly and can be dragged with the mouse (`wwwroot/js/carousel.js`): the row has copies of the first and last games at its ends, and when you reach a copy it jumps back by one lap, which you cannot see. A row of 4 games or less does not loop. The search bar of the home stays under the navbar while scrolling.

## Side menu

On phones it opens from the ☰ button, from `md` up it is always visible. "Cerca" puts the cursor in the search field of the page you are on (home or `/search`) through `SearchFocusService`; on other pages it opens the home and the field is focused as soon as it appears.

Logged in, the menu shows the user and "Esci"; logged out, "Accedi" and "Registrati". The navbar shows avatar and nickname (or the email when there is no nickname), linked to `/profile`.

## Tests

bUnit and xUnit on the pages with `FakeGameApiClient`: no API, database or Docker needed.

```bash
dotnet test fe/GameExplorer.Client.Tests
```

They cover the home (one row failing, the others loading), search (filters read from the address, invalid years and parameters), game page (not found, API down and retry, changing game), save buttons (saved, failed, not logged in), registration and profile. The [frontend workflow](../.github/workflows/frontend.yml) runs them on every PR that touches `fe/` or `shared/`.
