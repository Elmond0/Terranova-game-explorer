#!/usr/bin/env bash
# Run by the postgres image ONLY on first start (empty volume), after 01-app-user.sh.
# Creates the runtime user of the backend: it can read and write rows,
# but it cannot create, alter or drop tables (those belong to APP_DB_USER).
set -euo pipefail

psql -v ON_ERROR_STOP=1 \
     --username "$POSTGRES_USER" \
     --dbname "$POSTGRES_DB" \
     -v api_user="$API_DB_USER" \
     -v api_password="$API_DB_PASSWORD" \
     -v app_user="$APP_DB_USER" \
     -v db_name="$POSTGRES_DB" <<'EOSQL'
    CREATE ROLE :"api_user" LOGIN PASSWORD :'api_password'
        NOSUPERUSER NOCREATEDB NOCREATEROLE;

    GRANT CONNECT ON DATABASE :"db_name" TO :"api_user";
    GRANT USAGE ON SCHEMA public TO :"api_user";

    -- Tables and sequences that APP_DB_USER creates later (EF Core migrations)
    -- are automatically readable and writable by the runtime user.
    ALTER DEFAULT PRIVILEGES FOR ROLE :"app_user" IN SCHEMA public
        GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :"api_user";
    ALTER DEFAULT PRIVILEGES FOR ROLE :"app_user" IN SCHEMA public
        GRANT USAGE, SELECT ON SEQUENCES TO :"api_user";
EOSQL
