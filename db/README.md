# Local database

PostgreSQL 18 in Docker Compose, for **local development only** (not production).

Run all commands from the repository root.

## Requirements

- Docker Engine **25+** and Docker Compose **2.20.2+** (check with `docker version` and `docker compose version`)

## First start

```bash
cp .env.example .env
```

Fill in the three required passwords in `.env` (generate each one with `openssl rand -hex 24`):

- `POSTGRES_PASSWORD` — admin `terranova` (superuser), for maintenance and backups
- `APP_DB_PASSWORD` — migrations and restore user `terranova_app` (not a superuser, owner of the database)
- `API_DB_PASSWORD` — runtime user of the backend `terranova_api` (rows only)

Then:

```bash
docker compose up -d --wait   # returns when the database is healthy
docker compose ps             # db should be "healthy"
```

The database listens only on `127.0.0.1:5432` (change `POSTGRES_PORT` in `.env` if 5432 is already in use).

> Passwords and `db/init/` scripts apply **only on the first start** of the volume.
> If you change them later, reset the database (see [Stop vs reset](#stop-vs-reset)).

## Database users

| User | Used by | Can do |
|---|---|---|
| `terranova` | you: maintenance and backups | everything (superuser) |
| `terranova_app` | `migrate.sh` and restore | create, alter and drop tables (owner of the database) |
| `terranova_api` | the backend, at runtime | `SELECT`, `INSERT`, `UPDATE`, `DELETE` on rows; **not** `CREATE`, `ALTER`, `DROP`, `TRUNCATE` |

`db/init/01-app-user.sh` creates `terranova_app`, `db/init/02-api-user.sh` creates `terranova_api`.
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
docker compose --profile tools up -d --wait
```

Open http://localhost:8080 (port: `ADMINER_PORT` in `.env`) and log in with:

| Field    | Value                                                                    |
|----------|--------------------------------------------------------------------------|
| System   | **PostgreSQL** (the default is MySQL)                                    |
| Server   | `db`                                                                     |
| Username | `terranova`, `terranova_app` or `terranova_api`                          |
| Password | `POSTGRES_PASSWORD`, `APP_DB_PASSWORD` or `API_DB_PASSWORD` from `.env`  |
| Database | `terranova`                                                              |

Stop it when done:

```bash
docker compose --profile tools stop adminer
```

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

## Stop vs reset

```bash
docker compose down      # removes containers, KEEPS the data
docker compose down -v   # removes containers AND DELETES ALL DATA
```

Use `down -v` only to start from scratch, for example after changing passwords in `.env`.
If Adminer is running, add `--profile tools` (e.g. `docker compose --profile tools down`).

## Backend secrets (User Secrets)

The backend needs two values that are never stored in the repository: the connection string and the RAWG API key.
They go in .NET User Secrets, a file in your home folder (`~/.microsoft/usersecrets/<UserSecretsId>/secrets.json`), read only in development.

Set them **once per computer** (not per clone), from the repository root:

```bash
# Connection string: reads API_DB_PASSWORD from .env, so the password is not typed or kept in the shell history
dotnet user-secrets set ConnectionStrings:Default "Host=localhost;Port=5432;Database=terranova;Username=terranova_api;Password=$(grep '^API_DB_PASSWORD=' .env | cut -d= -f2- | tr -d '\r')" --project be/GameExplorer.Api

# RAWG API key (free, from https://rawg.io/apidocs): typed without echo
read -rsp "RAWG key: " key && dotnet user-secrets set Rawg:ApiKey "$key" --project be/GameExplorer.Api; unset key
# fish: read -s -P "RAWG key: " key; and dotnet user-secrets set Rawg:ApiKey $key --project be/GameExplorer.Api; set -e key

# Check (key names only, no values)
dotnet user-secrets list --project be/GameExplorer.Api | sed 's/ = .*//'
```

On **Windows (PowerShell 7)** `grep`, `cut`, `sed` and `read` do not exist; use these instead:

```powershell
# Connection string
$pw = (Select-String -Path .env -Pattern '^API_DB_PASSWORD=' | Select-Object -First 1).Line.Split('=', 2)[1]
dotnet user-secrets set ConnectionStrings:Default "Host=localhost;Port=5432;Database=terranova;Username=terranova_api;Password=$pw" --project be/GameExplorer.Api
Remove-Variable pw

# RAWG API key: typed as *** (-MaskInput needs PowerShell 7, not Windows PowerShell 5.1)
$key = Read-Host "RAWG key" -MaskInput
if ($key) { dotnet user-secrets set Rawg:ApiKey $key.Trim() --project be/GameExplorer.Api }
Remove-Variable key

# Check (key names only, no values)
dotnet user-secrets list --project be/GameExplorer.Api | ForEach-Object { ($_ -split ' = ')[0] }
```

On Windows the file is `%APPDATA%\Microsoft\UserSecrets\<UserSecretsId>\secrets.json`.
The `.sh` scripts (`migrate.sh`, `backup.sh`) need **Git Bash** or WSL; in Git Bash the bash commands above work as they are.

- Do **not** run `dotnet user-secrets init`: the project already has a `UserSecretsId`, a second one would split the secrets.
- Without `ConnectionStrings:Default` the app (and `dotnet ef`) stops with `Missing ConnectionStrings:Default`; without `Rawg:ApiKey` the app does not start.
- From a container on the `backend` network the host is `db` instead of `localhost` (see the bottom of [`.env.example`](../.env.example)).
- Online there are no User Secrets: the same values are environment variables, with `__` instead of `:` (`ConnectionStrings__Default`, `Rawg__ApiKey`).

Never use `terranova_app` here: its password is needed only by `migrate.sh` and restore.
