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
