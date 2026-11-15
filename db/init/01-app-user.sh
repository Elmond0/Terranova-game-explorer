#!/usr/bin/env bash

set -eu
app_password="$(cat /run/secrets/app_db_password)"
: "${app_password:?/run/secrets/app_db_password is empty}"

# Plain SQL instead of EF Core: EF Core creates tables but logs in with this user,
# so the user must already exist. We create it here, on the first start of the database.

psql -v ON_ERROR_STOP=1 \
	--username "$POSTGRES_USER" \
	--dbname "$POSTGRES_DB" \
	-v app_user="$APP_DB_USER" \
	-v app_password="$app_password" \
	-v db_name="$POSTGRES_DB" << 'EOSQL'

	-- 1. Create the migrations role
	CREATE ROLE :"app_user" LOGIN PASSWORD :'app_password'
		NOSUPERUSER NOCREATEDB NOCREATEROLE;

	-- 2. Make it the database owner
	ALTER DATABASE :"db_name" OWNER TO :"app_user";

	-- 3. Only roles granted CONNECT can log in
	REVOKE ALL ON DATABASE :"db_name" FROM PUBLIC;
EOSQL
