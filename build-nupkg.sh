#!/bin/sh

# NuGetBulkDownloader
# Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
# Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo
echo "==========================================================="
echo "Build FlashCap"
echo

# git clean -xfd

dotnet restore
dotnet build -p:Configuration=Release -p:Platform=AnyCPU nugetbd/nugetbd.csproj
dotnet build -p:Configuration=Release -p:Platform=AnyCPU nugetbd-cli/nugetbd-cli.csproj
dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts nugetbd-cli/nugetbd-cli.csproj
