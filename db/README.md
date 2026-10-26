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

## Backend connection string

Store it in .NET User Secrets, never in the repository. Examples are at the bottom of [`.env.example`](../.env.example):

- from your machine: `Host=localhost;Port=5432;Database=terranova;Username=terranova_api;Password=...`
- from a container on the `backend` network: `Host=db;Port=5432;...`

Never use `terranova_app` here: its password is needed only by `migrate.sh` and restore.
