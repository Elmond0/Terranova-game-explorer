#!/usr/bin/env bash
# Backup to db/backups/terranova_<date>.sql — usage: ./db/scripts/backup.sh
set -euo pipefail
cd "$(dirname "$0")/../.."
mkdir -p db/backups
file="db/backups/terranova_$(date +%Y%m%d_%H%M%S).sql"
# Credentials read inside the container (single quotes: sh expands the variables there).
# Write to .tmp first: if pg_dump fails, no half-written backup is left looking valid.
docker compose exec -T db sh -c \
  'pg_dump --clean --if-exists --no-owner -U "$POSTGRES_USER" -d "$POSTGRES_DB"' \
  > "$file.tmp"
mv "$file.tmp" "$file"
echo "Backup created: $file"
