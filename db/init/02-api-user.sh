#!/usr/bin/env bash

set -eu

api_password="$(cat /run/secrets/api_db_password)"
: "${api_password:?/run/secrets/api_db_password is empty}"

# The Web API gets its own user that can only read and write data, not change or drop tables,
# so a bug or a leaked password in the API cannot break the database. Plain SQL, like in 01.

psql -v ON_ERROR_STOP=1 \
	--username "$POSTGRES_USER" \
	--dbname "$POSTGRES_DB" \
	-v api_user="$API_DB_USER" \
	-v api_password="$api_password" \
	-v app_user="$APP_DB_USER" \
	-v db_name="$POSTGRES_DB" << 'EOSQL'
	-- 1. Create the Web API role
	CREATE ROLE :"api_user" LOGIN PASSWORD :'api_password'
		NOSUPERUSER NOCREATEDB NOCREATEROLE;

	-- 2. Let it connect and use schema public (01 removed CONNECT from everyone)
	GRANT CONNECT ON DATABASE :"db_name" TO :"api_user";
	GRANT USAGE ON SCHEMA public TO :"api_user";

	-- 3. Tables and sequences that the migrations create later get read/write rights automatically
	ALTER DEFAULT PRIVILEGES FOR ROLE :"app_user" IN SCHEMA public
		GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO :"api_user";
	ALTER DEFAULT PRIVILEGES FOR ROLE :"app_user" IN SCHEMA public
		GRANT USAGE, SELECT ON SEQUENCES TO :"api_user";
EOSQL
