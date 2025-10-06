#!/usr/bin/env bash
set -euo pipefail

echo "▶ Starting Jules .NET 9 + Docker SQL setup..."

# -------- Config --------
DOTNET_CHANNEL="${DOTNET_CHANNEL:-9.0}"
DOTNET_INSTALL_DIR="${DOTNET_INSTALL_DIR:-/app/.dotnet}"
DOTNET_TOOLS_PATH="${DOTNET_TOOLS_PATH:-${DOTNET_INSTALL_DIR}/tools}"
QUALITY="${QUALITY:-ga}"                      # ga|preview|daily
PROFILE_PERSIST="${PROFILE_PERSIST:-true}"   # persist PATH/DOTNET_ROOT to profile
COMPOSE_FILE="${COMPOSE_FILE:-docker-compose.yml}"

# CI-friendly env
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_MULTILEVEL_LOOKUP=0

# -------- Helpers --------
fail() { echo "✖ $*" >&2; exit 1; }
ok()   { echo "✓ $*"; }

trap 'echo "✖ Setup failed on line $LINENO"; exit 1' ERR

need_cmd() { command -v "$1" >/dev/null 2>&1 || fail "Missing required command: $1"; }

# -------- Pre-flight --------
need_cmd curl
need_cmd tar
need_cmd bash

mkdir -p "${DOTNET_INSTALL_DIR}" "${DOTNET_TOOLS_PATH}"

export DOTNET_ROOT="${DOTNET_INSTALL_DIR}"
export PATH="${DOTNET_ROOT}:${DOTNET_TOOLS_PATH}:${PATH}"

echo "• DOTNET_CHANNEL=${DOTNET_CHANNEL}"
echo "• DOTNET_ROOT=${DOTNET_ROOT}"
echo "• DOTNET_TOOLS_PATH=${DOTNET_TOOLS_PATH}"

# -------- Install .NET SDK --------
INSTALLER="/tmp/dotnet-install.sh"
echo "▶ Downloading dotnet-install.sh ..."
curl -fsSL https://dot.net/v1/dotnet-install.sh -o "${INSTALLER}"
chmod +x "${INSTALLER}"

echo "▶ Installing .NET SDK ${DOTNET_CHANNEL} (${QUALITY}) ..."
"${INSTALLER}" --channel "${DOTNET_CHANNEL}" --quality "${QUALITY}" --install-dir "${DOTNET_INSTALL_DIR}" --no-path
ok ".NET SDK installed/updated"

echo "▶ Verifying dotnet ..."
dotnet --info >/dev/null
ok "dotnet is available"

echo "▶ Installing/updating dotnet-ef ..."
if "${DOTNET_TOOLS_PATH}/dotnet-ef" --version >/dev/null 2>&1; then
  dotnet tool update dotnet-ef --tool-path "${DOTNET_TOOLS_PATH}" >/dev/null
else
  dotnet tool install dotnet-ef --tool-path "${DOTNET_TOOLS_PATH}" >/dev/null
fi
ok "dotnet-ef ready"

# -------- Persist PATH (optional) --------
if [ "${PROFILE_PERSIST}" = "true" ]; then
  SNIP='# >>> dotnet (Jules) >>>
export DOTNET_ROOT="{DOTNET_ROOT}"
export DOTNET_MULTILEVEL_LOOKUP=0
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export PATH="${DOTNET_ROOT}:${DOTNET_ROOT}/tools:${PATH}"
# <<< dotnet (Jules) <<<'

  SNIP="${SNIP//\{DOTNET_ROOT\}/${DOTNET_ROOT}}"
  for f in "$HOME/.bashrc" "$HOME/.profile"; do
    [ -f "$f" ] || continue
    if ! grep -q '>>> dotnet (Jules) >>>' "$f"; then
      echo "▶ Persisting PATH/DOTNET_ROOT in $f"
      printf "\n%s\n" "$SNIP" >> "$f"
    fi
  done
  ok "PATH persisted"
fi

# -------- Docker sanity & compose up --------
echo "▶ Checking Docker ..."
if ! command -v docker >/dev/null 2>&1; then
  fail "Docker is not installed. Install Docker Engine and re-run."
fi

# Try to talk to daemon
if ! docker info >/dev/null 2>&1; then
  echo "• Docker daemon not accessible. Attempting to enable..."
  if command -v systemctl >/dev/null 2>&1; then
    sudo systemctl enable --now docker || true
  fi
fi

if ! docker info >/dev/null 2>&1; then
  cat <<'EONOTE'
✖ Cannot talk to Docker daemon.
Quick fix (Linux):
  sudo usermod -aG docker "$USER"
  newgrp docker
  sudo systemctl enable --now docker
Then re-run this script.
EONOTE
  exit 1
fi
ok "Docker daemon reachable"

# Bring up SQL container
echo "▶ Starting Docker SQL via compose (${COMPOSE_FILE}) ..."
if [ ! -f "${COMPOSE_FILE}" ]; then
  fail "Compose file not found: ${COMPOSE_FILE}"
fi
docker compose -f "${COMPOSE_FILE}" up -d
ok "Compose started"

# -------- Wait for SQL to accept connections --------
SQL_HOST="${SQL_HOST:-localhost}"
SQL_PORT="${SQL_PORT:-1433}"
SQL_USER="${SQL_USER:-sa}"
SQL_PASSWORD="${SA_PASSWORD:-YourStrong(!)Password123}"
DB_NAME="${DB_NAME:-CMetalsWS}"

echo "▶ Waiting for SQL to become ready on ${SQL_HOST}:${SQL_PORT} ..."

# We will use the mssql-tools image to run sqlcmd against the host
MAX_TRIES=30
i=1
until docker run --rm --network host mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd -S "${SQL_HOST},${SQL_PORT}" -U "${SQL_USER}" -P "${SQL_PASSWORD}" -Q "SELECT 1" >/dev/null 2>&1
do
  if [ $i -ge $MAX_TRIES ]; then
    fail "SQL did not become ready after ${MAX_TRIES} attempts."
  fi
  sleep 2
  i=$((i+1))
done
ok "SQL is accepting connections"

# -------- Ensure database exists --------
echo "▶ Ensuring database '${DB_NAME}' exists ..."
docker run --rm --network host mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd   -S "${SQL_HOST},${SQL_PORT}" -U "${SQL_USER}" -P "${SQL_PASSWORD}"   -Q "IF DB_ID('${DB_NAME}') IS NULL BEGIN CREATE DATABASE [${DB_NAME}]; END"
ok "Database ready: ${DB_NAME}"

# -------- Final summary --------
echo ""
echo "--- Setup Verification ---"
echo -n "✓ dotnet version     : " && dotnet --version
echo -n "✓ dotnet-ef version  : " && "${DOTNET_TOOLS_PATH}/dotnet-ef" --version
echo "✓ Docker containers  :"
docker ps --format "table {{.Names}}	{{.Status}}	{{.Ports}}"

echo ""
echo "✅ Jules environment ready."
echo "   Use ASPNETCORE_ENVIRONMENT=Jules to point your app at the Docker SQL."
