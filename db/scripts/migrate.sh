#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/../.."
sql="$(mktemp)"
trap 'rm -f "$sql"' EXIT
dotnet ef migrations script --idempotent --project be/GameExplorer.Api --output "$sql"
echo 'REVOKE INSERT, UPDATE, DELETE ON "__EFMigrationsHistory" FROM :"api_user";' >> "$sql"
docker compose exec -T db sh -c \
  'psql -v ON_ERROR_STOP=1 -v api_user="$API_DB_USER" -U "$APP_DB_USER" -d "$POSTGRES_DB"' \
  < "$sql"
echo "Migrations applied."
