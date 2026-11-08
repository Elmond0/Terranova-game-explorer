# Terranova Game Explorer

Web app to explore video games: search and game pages, plus accounts with favorites, wishlist and tier lists. Game data comes from IGDB.

## How the project is divided

Two separate apps and one database:

```
Browser ⇄ Blazor WebAssembly client ──HTTP──▶ ASP.NET Core Web API ──▶ PostgreSQL
                                                      └──────────────▶ IGDB and other external APIs
```

| Folder | What it contains |
|---|---|
| `be/GameExplorer.Api` | Web API: endpoints, services for the external APIs, cache, Entity Framework, login |
| `fe/GameExplorer.Client` | Blazor WebAssembly client: pages, components, HTTP client towards the API |
| `shared/GameExplorer.Shared` | Models exchanged between API and client (the contract between the two apps) |
| `db/` | database setup: init scripts, migration and backup scripts, [db/README.md](db/README.md) |
| `DOCS/` | plans, decisions ([ADR](DOCS/adr/)) and team rules |

Rules that follow from this split:

- Only the Web API talks to the database and to the external APIs. The client talks only to the API.
- The client runs in the browser and anyone can read it: no keys or passwords in it.
- Passwords and keys live in `secrets/` (git-ignored), never in the repository.

| Area | Who |
|---|---|
| Data and services (Web API) | [@K0ff33Blu3](https://github.com/K0ff33Blu3) |
| Interface and graphics (client) | [@fmartini02](https://github.com/fmartini02) |
| Database and accounts | [@Elmond0](https://github.com/Elmond0) |

Branch, commit and pull request rules: [DOCS/come_lavoriamo.md](DOCS/come_lavoriamo.md). Full role split: [DOCS/Suddivisione del lavoro.md](DOCS/Suddivisione%20del%20lavoro.md).

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version`)
- Docker Engine 25+ with Docker Compose 2.20.2+ (`docker version`, `docker compose version`)
- Git
- `make` (optional): on Linux install it with your package manager, on macOS `xcode-select --install`
- Windows only: [Git for Windows](https://git-scm.com/download/win) (gives you Git Bash, needed by the `.sh` scripts) and [PowerShell 7](https://learn.microsoft.com/powershell/scripting/install/installing-powershell-on-windows). `make` is optional: `winget install ezwinports.make`, then open a new terminal. Without it, use the plain commands below.

## Run it

1. Create these five files in `secrets/` (three database passwords and the two IGDB keys). The names must match exactly, `.txt` included:

   | File | Content |
   |---|---|
   | `secrets/postgres_password.txt` | password of the PostgreSQL superuser |
   | `secrets/app_db_password.txt` | password of the application database user |
   | `secrets/api_db_password.txt` | password of the Web API database user |
   | `secrets/twitch_id.txt` | IGDB / Twitch Client ID |
   | `secrets/twitch_key.txt` | IGDB / Twitch Client Secret |

   Commands for Linux, macOS and Windows: [db/README.md → Secret files](db/README.md#secret-files). `make secrets` checks that all five exist.
2. Start the database and apply the migrations:

   ```bash
   make
   ```

3. Start the API and the client together:

   ```bash
   make dev
   ```

   - Web API: http://localhost:5290
   - Client: http://localhost:5100

   `Ctrl+C` stops both.

Without `make` (for example on Windows without it), the same steps are:

```bash
cp .env.example .env          # PowerShell: Copy-Item .env.example .env
docker compose up -d --wait db
```

then the migrations ([db/README.md → Migrations](db/README.md#migrations)) and, in two terminals, the API and the client with the variables described in [db/README.md → Backend secrets](db/README.md#backend-secrets) (bash and PowerShell commands included):

```bash
dotnet run --launch-profile http --project be/GameExplorer.Api
dotnet run --launch-profile http --project fe/GameExplorer.Client
```

All the database commands (Adminer, backup and restore, reset) are in [db/README.md](db/README.md). `make clean` stops the database and keeps the data.

## Tests

Docker must be running: the API tests start their own temporary PostgreSQL.

```bash
make test
# or, without make (same on Windows):
dotnet test GameExplorer.slnx
```
