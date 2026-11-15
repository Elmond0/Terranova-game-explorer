#!/usr/bin/env bash

set -eu
cd "$(dirname "$0")/../.."
sql="$(mktemp)"
trap 'rm -f "$sql"' EXIT

# Not "dotnet ef database update": EF Core only writes the SQL, then psql runs it inside the container
# as the migrations user, so its password is never needed on your PC. The API must not edit the migration history.

dotnet ef migrations script --idempotent --project be/GameExplorer.Api --output "$sql"
echo 'REVOKE INSERT, UPDATE, DELETE ON "__EFMigrationsHistory" FROM :"api_user";' >> "$sql"
docker compose exec -T db sh -c \
  'psql -v ON_ERROR_STOP=1 -v api_user="$API_DB_USER" -U "$APP_DB_USER" -d "$POSTGRES_DB"' \
  < "$sql"
echo "Migrations applied."
