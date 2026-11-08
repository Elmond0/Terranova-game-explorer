# Local database

PostgreSQL 18 in Docker Compose, for **local development only** (not production).

Run all commands from the repository root.

## Requirements

- Docker Engine **25+** and Docker Compose **2.20.2+** (check with `docker version` and `docker compose version`)
- .NET **10** SDK, for the migrations (check with `dotnet --version`)
- A Twitch account with 2FA enabled, to get the IGDB keys (see [Secret files](#secret-files))
- `make` (optional, it makes the start a single command): on Linux install it with your package manager (`make` or `build-essential`), on macOS it comes with `xcode-select --install`. Every `make` target below also has its plain commands next to it.
- Windows only:
  - **Git Bash** (comes with [Git for Windows](https://git-scm.com/download/win)), to run the `.sh` scripts
  - **PowerShell 7**, not Windows PowerShell 5.1, for the PowerShell commands in this file (`winget install Microsoft.PowerShell`)
  - `make`: `winget install ezwinports.make`, then open a new terminal. Skipping it is fine: use the plain commands

## Secret files

Passwords and keys live in the `secrets/` folder at the repository root, one line per file, **never committed** (it is in `.gitignore`).
You create the files once; `make` stops and names the missing one if you forget.

| File | Content | Used by |
|---|---|---|
| `secrets/postgres_password.txt` | password of `terranova` (admin) | the `db` container |
| `secrets/app_db_password.txt` | password of `terranova_app` (migrations) | the `db` container |
| `secrets/api_db_password.txt` | password of `terranova_api` (backend) | the `db` container and the backend connection string |
| `secrets/twitch_id.txt` | Twitch **Client ID** | the backend (`Igdb__ClientId`) |
| `secrets/twitch_key.txt` | Twitch **Client Secret** | the backend (`Igdb__ClientSecret`) |

The Twitch keys are for IGDB: create an app in the [Twitch developer console](https://dev.twitch.tv/console) (the Client Secret is shown only once, copy it right away). The step-by-step guide is in `DOCS/qa-and-igdb-guide.md`.

Create the files (the three passwords are random; the Twitch values are typed without echo):

```bash
mkdir -p secrets
for f in postgres_password app_db_password api_db_password; do
  openssl rand -hex 24 | tr -d '\n' > secrets/$f.txt
done
read -rsp "Twitch Client ID: " v && printf %s "$v" > secrets/twitch_id.txt; unset v
read -rsp "Twitch Client Secret: " v && printf %s "$v" > secrets/twitch_key.txt; unset v
chmod 644 secrets/*.txt   # Linux: the postgres user in the container must be able to read them
```

On **Windows (PowerShell 7)** `openssl`, `tr` and `read` are not available; use this instead:

```powershell
New-Item -ItemType Directory -Force secrets | Out-Null
foreach ($f in 'postgres_password', 'app_db_password', 'api_db_password') {
    Set-Content -NoNewline "secrets/$f.txt" ([Convert]::ToHexString([Security.Cryptography.RandomNumberGenerator]::GetBytes(24)).ToLower())
}
Set-Content -NoNewline secrets/twitch_id.txt (Read-Host 'Twitch Client ID' -MaskInput)
Set-Content -NoNewline secrets/twitch_key.txt (Read-Host 'Twitch Client Secret' -MaskInput)
```

Rules for these files:

- Use only letters and digits in the passwords: they end up in a connection string, where `;` and `=` break it. The commands above already do.
- One line, no trailing spaces. If you write a file with an editor on Windows, save it with **LF** line endings, otherwise a hidden `\r` becomes part of the password.
- To change a password later you must reset the database, because the passwords are read only on the first start (see [Stop vs reset](#stop-vs-reset)).

## First start

```bash
make
```

`make` does everything in order: creates `.env` from `.env.example` if missing, checks the [secret files](#secret-files), starts the database and applies the [migrations](#migrations). Running it again is safe.

Without `make`, the same steps are:

```bash
cp .env.example .env          # Windows PowerShell: Copy-Item .env.example .env
docker compose up -d --wait   # returns when the database is healthy
docker compose ps             # db should be "healthy"
```

then the [migrations](#migrations) (and the [backend secrets](#backend-secrets) when you start the API).

`.env` holds only non-secret settings (names and ports), so it is safe to leave as it is. The database listens only on `127.0.0.1:5432` (change `POSTGRES_PORT` in `.env` if 5432 is already in use).

> Passwords and `db/init/` scripts apply **only on the first start** of the volume.
> If you change them later, reset the database (see [Stop vs reset](#stop-vs-reset)).

## Make commands

| Command | What it does |
|---|---|
| `make` | `.env`, secret check, database, migrations |
| `make up` | only `.env`, secret check and database |
| `make secrets` | only checks that the five secret files exist and are not empty |
| `make migrate` | applies the missing migrations (passes the [backend secrets](#backend-secrets)) |
| `make dev` | Web API and client together, with the [backend secrets](#backend-secrets) (`Ctrl+C` stops both) |
| `make test` | all the tests (Docker must be running: they start their own temporary database) |
| `make adminer` | database and Adminer |
| `make adminer-stop` | stops and removes only Adminer, the database keeps running |
| `make ps` | status of the containers |
| `make logs` | follows the logs (`Ctrl+C` to leave) |
| `make clean` | stops the containers, **keeps** the data |
| `make fclean` | **deletes the local data** (asks for `yes`); the `secrets/` files stay |
| `make re` | `fclean`, then `make` |

## Database users

| User | Used by | Can do |
|---|---|---|
| `terranova` | you: maintenance and backups | everything (superuser) |
| `terranova_app` | `migrate.sh` and restore | create, alter and drop tables (owner of the database) |
| `terranova_api` | the backend, at runtime | `SELECT`, `INSERT`, `UPDATE`, `DELETE` on rows; **not** `CREATE`, `ALTER`, `DROP`, `TRUNCATE` |

`db/init/01-app-user.sh` creates `terranova_app`, `db/init/02-api-user.sh` creates `terranova_api`. Both read the password from the secret files that Docker mounts in the container (`/run/secrets/`).
Tables that `terranova_app` creates later (EF Core migrations) are readable and writable by `terranova_api` automatically (`ALTER DEFAULT PRIVILEGES`).

The backend always connects as `terranova_api`: if it has a bug, it cannot change or drop tables.

## Everyday commands

```bash
docker compose ps               # status
docker compose logs -f db       # logs (queries slower than 200 ms are logged here)
docker compose stop             # stop, keep the containers
docker compose start            # start again
```

## Connect with psql

```bash
docker compose exec db psql -U terranova -d terranova
```

Useful commands inside psql: `\dt` tables · `\d <table>` structure · `\dx` extensions · `\du` users · `\q` quit.

## Adminer (web UI)

```bash
make adminer   # or: docker compose --profile tools up -d --wait
```

Open http://localhost:8080 (port: `ADMINER_PORT` in `.env`) and log in with:

| Field    | Value                                                                                          |
|----------|------------------------------------------------------------------------------------------------|
| System   | **PostgreSQL** (the default is MySQL)                                                          |
| Server   | `db`                                                                                           |
| Username | `terranova`, `terranova_app` or `terranova_api`                                                |
| Password | the content of `secrets/postgres_password.txt`, `secrets/app_db_password.txt` or `secrets/api_db_password.txt` |
| Database | `terranova`                                                                                    |

Stop it when done:

```bash
make adminer-stop   # or: docker compose --profile tools rm -sf adminer
```

The database keeps running.

## Backup and restore

Backup (the database must be running):

```bash
./db/scripts/backup.sh   # creates db/backups/terranova_<date>_<time>.sql
```

`db/backups/` is ignored by git: backups contain real data, never commit them.

Restore (as the migrations user `terranova_app`, so restored tables belong to it and stay readable by `terranova_api`):

```bash
docker compose exec -T db sh -c 'psql -v ON_ERROR_STOP=1 -U "$APP_DB_USER" -d "$POSTGRES_DB"' < db/backups/terranova_20261025_210317.sql
```

Replace the file name with the backup you want to restore.

On Windows, run `backup.sh` in Git Bash. The restore redirect (`<`) does not work in PowerShell: run it in Git Bash too.

## Stop vs reset

```bash
make clean   # removes containers, KEEPS the data  (docker compose --profile tools down)
make fclean  # removes containers AND DELETES ALL DATA, asks for "yes"  (docker compose --profile tools down -v)
```

Use `fclean` only to start from scratch, for example after changing a password in `secrets/`.
After it, `make` creates the database again with the passwords that are in `secrets/` at that moment.
The `secrets/` files are never deleted by `make`: delete them by hand if you want new ones.

## Backend secrets

The backend needs three values that are never stored in the repository: the connection string and the two IGDB keys.
`make dev` and `make migrate` build them from the [secret files](#secret-files) and pass them to the backend as environment variables, so no password is typed or kept in the shell history. There are no .NET User Secrets: everything lives in `secrets/`.

| Variable | Value |
|---|---|
| `ConnectionStrings__Default` | host, port, database and user from `.env`, password from `secrets/api_db_password.txt` |
| `Igdb__ClientId` | `secrets/twitch_id.txt` |
| `Igdb__ClientSecret` | `secrets/twitch_key.txt` |

(`__` stands for `:` in configuration names: `ConnectionStrings__Default` is `ConnectionStrings:Default`.)

The backend sees them **only when it is started by `make`** (or when you set the variables yourself). An IDE or a plain `dotnet run` does not: use `make dev`, or set them in the same terminal first:

```bash
export ConnectionStrings__Default="Host=localhost;Port=5432;Database=terranova;Username=terranova_api;Password=$(< secrets/api_db_password.txt)"
export Igdb__ClientId="$(< secrets/twitch_id.txt)"
export Igdb__ClientSecret="$(< secrets/twitch_key.txt)"
dotnet run --launch-profile http --project be/GameExplorer.Api
# fish: set -x Igdb__ClientId (cat secrets/twitch_id.txt), and so on
```

On **Windows (PowerShell 7)** `$(< file)` does not exist; use this instead:

```powershell
$pw = (Get-Content -Raw secrets/api_db_password.txt).Trim()
$env:ConnectionStrings__Default = "Host=localhost;Port=5432;Database=terranova;Username=terranova_api;Password=$pw"
$env:Igdb__ClientId = (Get-Content -Raw secrets/twitch_id.txt).Trim()
$env:Igdb__ClientSecret = (Get-Content -Raw secrets/twitch_key.txt).Trim()
Remove-Variable pw
dotnet run --launch-profile http --project be/GameExplorer.Api
```

The variables last only for that terminal window. In an IDE, set the same three variables in the run configuration of the API project (do not commit them).

- Without `ConnectionStrings__Default` the app (and `dotnet ef`) stops with `Missing ConnectionStrings:Default`.
- From a container the host is `db` instead of `localhost`.
- Online the same variables are used, set by the hosting service.

Never use `terranova_app` here: its password is needed only by `migrate.sh` and restore.

## Migrations

The tables are created only by EF Core migrations (`be/GameExplorer.Api/Data/Migrations/`), applied by `db/scripts/migrate.sh` as `terranova_app`.

### Apply them (everyone)

`make migrate` passes the [backend secrets](#backend-secrets) and runs the three commands below (without `make`, set the variables first: `dotnet ef` stops without them). They are the same on Linux, macOS and Windows (in Git Bash):

```bash
dotnet tool restore                  # once per clone: installs dotnet-ef from dotnet-tools.json
dotnet restore be/GameExplorer.Api   # once per clone: downloads the NuGet packages dotnet-ef needs
./db/scripts/migrate.sh              # applies the missing migrations
```

Run it again after a `git pull` that brings new migrations: running it twice is safe, applied migrations are skipped.

Check that the tables exist:

```bash
docker compose exec -T db psql -U terranova -d terranova -c '\dt'
```

### Create one (only Elmond0)

Two migrations created in parallel conflict: if you need a table or a column, ask Elmond0.

```bash
dotnet ef migrations add AddSomething --project be/GameExplorer.Api      # after changing Data/
dotnet ef migrations script --idempotent --project be/GameExplorer.Api   # read the SQL before applying
./db/scripts/migrate.sh
```

A migration already on `main` is never edited: fix it with a new one.
