#!/usr/bin/env bash
set -euo pipefail

SQL_HOST="${SQL_HOST:-localhost}"
SQL_PORT="${SQL_PORT:-1433}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASSWORD="${SA_PASSWORD:-YourStrong(!)Password123}"
DB_NAME="${DB_NAME:-CMetalsWS}"

echo "Ensuring database '${DB_NAME}' exists at ${SQL_HOST}:${SQL_PORT} ..."

docker run --rm --network host mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd   -S "${SQL_HOST},${SQL_PORT}" -U "${SQL_USER}" -P "${SQL_PASSWORD}"   -Q "IF DB_ID('${DB_NAME}') IS NULL BEGIN CREATE DATABASE [${DB_NAME}]; END"

echo "OK."
