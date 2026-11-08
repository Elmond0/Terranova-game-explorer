#!/usr/bin/env bash

set -euo pipefail

api_password="$(cat /run/secrets/api_db_password)"
: "${api_password:?/run/secrets/api_db_password is empty}"

psql -v ON_ERROR_STOP=1 \
     --username "$POSTGRES_USER" \
     --dbname "$POSTGRES_DB" \
     -v api_user="$API_DB_USER" \
     -v api_password="$api_password" \
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
