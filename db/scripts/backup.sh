#!/usr/bin/env bash

set -eu
cd "$(dirname "$0")/../.."
mkdir -p db/backups
file="db/backups/terranova_$(date +%Y%m%d_%H%M%S).sql"

# pg_dump runs inside the container, so you don't need PostgreSQL on your PC (the $VARIABLES are read there).
# It writes to .tmp first: if the backup fails, no broken file is left that looks like a good one.
docker compose exec -T db sh -c \
  'pg_dump --clean --if-exists --no-owner -U "$POSTGRES_USER" -d "$POSTGRES_DB"' \
  > "$file.tmp"
mv "$file.tmp" "$file"
echo "Backup created: $file"
