#!/usr/bin/env bash

set -euo pipefail

app_password="$(cat /run/secrets/app_db_password)"
: "${app_password:?/run/secrets/app_db_password is empty}"

psql -v ON_ERROR_STOP=1 \
	--username "$POSTGRES_USER" \
	--dbname "$POSTGRES_DB" \
	-v app_user="$APP_DB_USER" \
	-v app_password="$app_password" \
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
