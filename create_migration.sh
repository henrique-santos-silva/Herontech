#!/usr/bin/env bash
# Uso: ./create_migration.sh NomeDaMigration
dotnet ef migrations add "$1" \
  --project Herontech.Infrastructure/Herontech.Infrastructure.csproj \
  --startup-project Herontech.Api/Herontech.Api.csproj
