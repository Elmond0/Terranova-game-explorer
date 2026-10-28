#!/usr/bin/env bash
# Apply EF Core migrations as APP_DB_USER (owner) — usage: ./db/scripts/migrate.sh
set -euo pipefail
cd "$(dirname "$0")/../.."
sql="$(mktemp)"
trap 'rm -f "$sql"' EXIT
# Idempotent: migrations already in __EFMigrationsHistory are skipped
dotnet ef migrations script --idempotent --project be/GameExplorer.Api --output "$sql"
# Run inside the container (trust on the local socket): no password needed here
docker compose exec -T db sh -c \
  'psql -v ON_ERROR_STOP=1 -U "$APP_DB_USER" -d "$POSTGRES_DB"' \
  < "$sql"
echo "Migrations applied."
