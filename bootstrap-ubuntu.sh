#!/usr/bin/env bash
# bootstrap-ubuntu.sh — Blazor .NET 9 + EF Tools + Docker SQL (hardened) — no global.json required
set -euo pipefail

echo "==> Preflight"
if [ -f /etc/os-release ]; then . /etc/os-release; else echo "Unsupported OS"; exit 1; fi
UBU_VER="${VERSION_ID:-22.04}"
ARCH_RAW="$(uname -m)"
ARCH="x64"
case "$ARCH_RAW" in
  x86_64) ARCH="x64" ;;
  aarch64) ARCH="arm64" ;;
esac
echo "Ubuntu ${UBU_VER} on ${ARCH_RAW}"

# --- .NET 9 SDK via APT (preferred) with fallback to official installer ---
echo "==> Installing .NET 9 SDK (APT first, fallback to official script)"
wget -q "https://packages.microsoft.com/config/ubuntu/${UBU_VER}/packages-microsoft-prod.deb" -O packages-microsoft-prod.deb || true
sudo dpkg -i packages-microsoft-prod.deb >/dev/null 2>&1 || true
sudo apt-get update -y || true

APT_OK=true
if ! sudo apt-get install -y dotnet-sdk-9.0; then
  APT_OK=false
fi

if ! dotnet --list-sdks 2>/dev/null | grep -E '^9\.' >/dev/null; then
  echo "APT did not provide a 9.x SDK; falling back to official installer…"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  chmod +x /tmp/dotnet-install.sh
  /tmp/dotnet-install.sh --channel 9.0 --quality GA --architecture "$ARCH" --install-dir "$HOME/.dotnet"
  export PATH="$HOME/.dotnet:$HOME/.dotnet/tools:$PATH"
  rm -f /tmp/dotnet-install.sh
fi

# Ensure tools path for current + future shells
if ! grep -q 'export PATH="$HOME/.dotnet/tools:$PATH"' "$HOME/.bashrc" 2>/dev/null; then
  echo 'export PATH="$HOME/.dotnet/tools:$PATH"' >> "$HOME/.bashrc"
fi
export PATH="$HOME/.dotnet/tools:$PATH"

# Verify .NET 9 present
dotnet --list-sdks | grep -E '^9\.' >/dev/null || { echo "ERROR: .NET 9 SDK not found after install"; exit 1; }
echo "dotnet version: $(dotnet --version)"

# --- Workloads & dev certs ---
echo "==> Installing WebAssembly workload"
dotnet workload install wasm-tools || true

echo "==> HTTPS dev cert (Linux: created but may not import system-wide)"
dotnet dev-certs https || true

# --- EF Core tools: prefer local manifest; else global. Pin to stable 9.* ---
EF_MODE="global"
if [ -f ".config/dotnet-tools.json" ]; then
  echo "==> Local tool manifest detected — pin EF Core CLI to 9.x"
  dotnet new tool-manifest --force >/dev/null 2>&1 || true
  dotnet tool update  --local dotnet-ef --version 9.* || dotnet tool install --local dotnet-ef --version 9.*
  dotnet tool restore
  EF_MODE="local"
else
  echo "==> No local tool manifest — installing global EF Core CLI 9.x"
  dotnet tool update -g dotnet-ef --version 9.* || dotnet tool install -g dotnet-ef --version 9.*
fi

# Verify EF tool
if [ "$EF_MODE" = "local" ]; then
  EF_VER="$(dotnet tool run dotnet-ef -- --version || true)"
else
  EF_VER="$(dotnet-ef --version || true)"
fi
echo "EF Core CLI (${EF_MODE}): ${EF_VER}"

# --- Docker Engine + Compose plugin (if missing) ---
if ! command -v docker >/dev/null 2>&1; then
  echo "==> Installing Docker Engine + compose plugin"
  sudo apt-get install -y ca-certificates curl gnupg lsb-release
  sudo apt-get update -y
  sudo apt-get install -y docker.io docker-compose-plugin
  sudo systemctl enable --now docker
  sudo usermod -aG docker "$USER" || true
fi
echo "docker: $(docker --version | cut -d',' -f1)"
docker compose version || true

# --- Hardened SQL Server container with sqlcmd healthcheck ---
echo "==> Writing db/Dockerfile (includes mssql-tools18 for real healthcheck)"
mkdir -p ./db
cat > ./db/Dockerfile <<'DOCKERFILE'
FROM mcr.microsoft.com/mssql/server:2022-latest
USER root
RUN apt-get update && \
    apt-get install -y curl apt-transport-https gnupg && \
    curl -fsSL https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    . /etc/os-release && \
    curl -fsSL "https://packages.microsoft.com/config/ubuntu/${VERSION_ID}/prod.list" > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev && \
    ln -s /opt/mssql-tools18/bin/sqlcmd /usr/local/bin/sqlcmd && \
    apt-get clean && rm -rf /var/lib/apt/lists/*
USER mssql
DOCKERFILE

echo "==> Writing docker-compose.sql.yml"
cat > ./docker-compose.sql.yml <<'YML'
version: "3.9"
services:
  sql-server-db:
    build:
      context: ./db
    image: cmetalsws-sql:secure
    container_name: cmetalsws-sql
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD:-YourStrong!Passw0rd}
    ports:
      - "1433:1433"
    volumes:
      - mssql:/var/opt/mssql
    healthcheck:
      test: ["CMD-SHELL", "sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q 'SELECT 1' >/dev/null 2>&1 || exit 1"]
      interval: 10s
      timeout: 5s
      retries: 15
      start_period: 20s
volumes:
  mssql:
YML

echo "==> Building and starting SQL Server"
docker compose -f docker-compose.sql.yml build --pull
docker compose -f docker-compose.sql.yml up -d

echo "==> Waiting for cmetalsws-sql to be healthy…"
for i in {1..30}; do
  state="$(docker inspect -f '{{.State.Health.Status}}' cmetalsws-sql 2>/dev/null || echo "starting")"
  echo "Health: $state"
  [ "$state" = "healthy" ] && break
  sleep 3
done

echo "==> Recent logs"
docker logs --tail 60 cmetalsws-sql || true

# --- Connection strings ---
echo
echo "======== Connection Strings ========"
echo "Host → Container:"
echo "Server=localhost,1433;Database=CMetalsWS;User Id=sa;Password=${SA_PASSWORD:-YourStrong!Passw0rd};Encrypt=True;TrustServerCertificate=True"
echo
echo "Container → Container (compose):"
echo "Server=sql-server-db,1433;Database=CMetalsWS;User Id=sa;Password=${SA_PASSWORD:-YourStrong!Passw0rd};Encrypt=True;TrustServerCertificate=True"
echo "===================================="
echo

# --- Optional: install sqlcmd on host for manual queries ---
if ! command -v sqlcmd >/dev/null 2>&1; then
  echo "==> Installing mssql-tools18 on host (optional)"
  sudo ACCEPT_EULA=Y apt-get install -y mssql-tools18 unixodbc-dev || true
  if ! grep -q '/opt/mssql-tools18/bin' "$HOME/.bashrc"; then
    echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> "$HOME/.bashrc"
  fi
  export PATH="$PATH:/opt/mssql-tools18/bin"
fi
# Quick test (non-fatal)
sqlcmd -S 127.0.0.1,1433 -U sa -P "${SA_PASSWORD:-YourStrong!Passw0rd}" -Q "SELECT @@VERSION" >/dev/null 2>&1 || true

# --- Final report & next steps ---
echo
echo "== Final Report =="
echo "dotnet: $(dotnet --version)"
echo "SDKs:"; dotnet --list-sdks | sed 's/^/  /'
echo "EF Core CLI mode: ${EF_MODE}"
if [ "$EF_MODE" = "local" ]; then
  echo "  (use: dotnet tool run dotnet-ef …)"
else
  echo "  (use: dotnet-ef …)"
fi
echo "docker: $(docker --version | cut -d',' -f1)"
echo "compose: $(docker compose version | head -n1)"
echo "DB container: $(docker inspect -f '{{.State.Status}} (health={{.State.Health.Status}})' cmetalsws-sql || echo 'not found')"
echo "===================================="
echo
echo "Next steps:"
echo "1) Update appsettings.json with the Host → Container connection string above."
echo "2) dotnet restore"
if [ "$EF_MODE" = "local" ]; then
  echo "3) dotnet tool run dotnet-ef database update   # from the project with your DbContext/Migrations"
else
  echo "3) dotnet-ef database update                   # from the project with your DbContext/Migrations"
fi
echo "4) dotnet run  (or your IDE)"
