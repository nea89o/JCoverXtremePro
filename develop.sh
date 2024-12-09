#!/bin/bash
set -euo pipefail

dotnet build
sudo cp ./bin/Debug/net8.0/Jellyfin.Plugin.JCoverXtremePro.* testenv/config/plugins/JCoverXtremePro
docker compose up

