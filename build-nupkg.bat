@echo off

rem NuGetBulkDownloader
rem Copyright (c) Kouji Matsui (@kozy_kekyo, @kekyo@mastodon.cloud)
rem Licensed under Apache-v2: https://opensource.org/licenses/Apache-2.0

echo.
echo "==========================================================="
echo "Build NuGetBulkDownloader"
echo.

rem git clean -xfd

dotnet restore
dotnet build -p:Configuration=Release -p:Platform=AnyCPU nugetbd-cli\nugetbd-cli.csproj
dotnet pack -p:Configuration=Release -p:Platform=AnyCPU -o artifacts nugetbd-cli\nugetbd-cli.csproj
