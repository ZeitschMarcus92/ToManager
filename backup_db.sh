#!/usr/bin/env bash
set -euo pipefail

SERVICE="db"
BACKUP_DIR="./backups"
TS="$(date +"%Y-%m-%d_%H%M")"
FILE="${BACKUP_DIR}/backup_${TS}.sql"

mkdir -p "$BACKUP_DIR"

CID="$(docker compose ps -q "$SERVICE" || true)"
if [[ -z "$CID" ]]; then
  echo "Database service '$SERVICE' not running. Start it with: docker compose up -d"
  exit 1
fi

RUNNING="$(docker inspect -f '{{.State.Running}}' "$CID")"
if [[ "$RUNNING" != "true" ]]; then
  echo "Database container is not running."
  exit 1
fi

DB_USER="$(docker exec "$CID" sh -lc 'printf "%s" "${POSTGRES_USER:-postgres}"')"
DB_NAME="$(docker exec "$CID" sh -lc 'printf "%s" "${POSTGRES_DB:-postgres}"')"

echo "Creating dump: $FILE"
echo "Using DB_USER=$DB_USER DB_NAME=$DB_NAME"

docker exec -i "$CID" pg_dump -U "$DB_USER" -d "$DB_NAME" > "$FILE"

ls -1t "${BACKUP_DIR}"/backup_*.sql 2>/dev/null | tail -n +6 | xargs -r rm -f

echo "Done."
