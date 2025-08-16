#!/usr/bin/env bash
# Uso: ./update_db.sh [NomeDaMigration]
dotnet ef database update ${1:-} \
  --project Herontech.Infrastructure/Herontech.Infrastructure.csproj \
  --startup-project Herontech.Api/Herontech.Api.csproj
