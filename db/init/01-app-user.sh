#!/usr/bin/env bash
# Runs ONLY on first start (empty volume).
set -euo pipefail
psql -v ON_ERROR_STOP=1 \
	--username "$POSTGRES_USER" \
	--dbname "$POSTGRES_DB" \
	-v app_user="$APP_DB_USER" \
	-v app_password="$APP_DB_PASSWORD" \
	-v db_name="$POSTGRES_DB" << 'EOSQL'
	-- 1. Create the backend role
	CREATE ROLE :"app_user" LOGIN PASSWORD :'app_password'
		NOSUPERUSER NOCREATEDB NOCREATEROLE;

	-- 2. Make it the database owner
	ALTER DATABASE :"db_name" OWNER TO :"app_user";
	-- 3. Nobody else can connect
	REVOKE ALL ON DATABASE :"db_name" FROM PUBLIC;

	-- 4. Create pg_trgm as the app user, so it owns it
	SET ROLE :"app_user";
	CREATE EXTENSION IF NOT EXISTS pg_trgm;
	RESET ROLE;
EOSQL
