// NuGetBulkDownloader
// Copyright (c) Kouji Matsui.
// License: Under Apache-v2
// https://github.com/kekyo/NuGetBulkDownloader

using Mono.Options;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetBulkDownloader;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var userName = default(string);
        var password = default(string);
        var includePrereleases = false;
        var performParallel = false;
        var basePath = "packages";

        Console.WriteLine($"NuGetBulkDownloader {ThisAssembly.AssemblyVersion}");
        Console.WriteLine($"Copyright (c) Kouji Matsui");
        Console.WriteLine($"https://github.com/kekyo/NuGetBulkDownloader");
        Console.WriteLine();

        var options = new OptionSet
        {
            {"i", "Include prerelease packages", _ => includePrereleases = true },
            {"p", "Perform parallel download", _ => performParallel = true },
            {"basePath=", "Store packages into this directory", v => basePath = v },
            {"userName=", "Basic authentication user name", v => userName = v},
            {"password=", "Basic authentication password", v => password = v},
        };

        var remains = options.Parse(args);
        if (remains.Count < 1)
        {
            Console.WriteLine($"usage: NuGetBulkDownloader [options] <nuget endpoint url>");
            options.WriteOptionDescriptions(Console.Out);
            return;
        }

        var serviceIndexUrl = remains[0];

        var httpAccessor = new HttpAccessor(userName, password);

        Console.Write($"Fetching nuget service endpoints from: {serviceIndexUrl} ...");
        var serviceIndex = await httpAccessor.QueryAsync<ServiceIndex>(serviceIndexUrl);
        Console.WriteLine(" Done.");

        if (serviceIndex?.resources?.
            FirstOrDefault(r => r._type?.StartsWith("SearchQueryService") ?? false) is { } searchQueryService &&
            serviceIndex?.resources?.
            FirstOrDefault(r => r._type?.StartsWith("PackageBaseAddress") ?? false) is { } packageBaseAddress)
        {
            // https://learn.microsoft.com/ja-jp/nuget/api/search-query-service-resource
            var searchQueryServiceUrl =
                $"{searchQueryService._id}?semVerLevel=2.0.0&prerelease={includePrereleases.ToString().ToLowerInvariant()}";

            var searchQueryResponse = await httpAccessor.QueryAsync<SearchQueryResponse>(searchQueryServiceUrl);

            var candidates =
                searchQueryResponse?.data?.
                Where(result => result?.id is { }).
                SelectMany(result =>
                    result.versions?.
                    Where(version => version?.version is { }).
                    Select(version =>
                    {
                        var fileName = $"{result.id}.{version.version}.nupkg";
                        var downloadUrl =
                            $"{packageBaseAddress._id}/{result.id!.ToLowerInvariant()}/{version.version!.ToLowerInvariant()}/{fileName.ToLowerInvariant()}";
                        var packageBasePath = Path.Combine(basePath, result.id);

                        return (fileName, downloadUrl, packageBasePath);
                    }) ??
                    Array.Empty<(string, string, string)>()).
                ToArray()!;

            Console.WriteLine($"Found {candidates.Length} valid package entries.");

            static async Task DownloadAsync(
                HttpAccessor httpAccessor,
                string fileName, string downloadUrl, string packageBasePath,
                bool performParallel)
            {
                if (!performParallel)
                {
                    Console.Write($"Downloading {fileName} ...");
                }

                using var response = await httpAccessor.RequestQueryAsync(downloadUrl);

                if (!Directory.Exists(packageBasePath))
                {
                    try
                    {
                        Directory.CreateDirectory(packageBasePath);
                    }
                    catch
                    {
                    }
                }

                var path = Path.Combine(packageBasePath, fileName);

                using var stream = await response.Content.ReadAsStreamAsync();
                using var f = new FileStream(
                    path,
                    FileMode.Create,
                    FileAccess.ReadWrite,
                    FileShare.None,
                    65536,
                    true);

                await stream.CopyToAsync(f);
                await f.FlushAsync();

                if (!performParallel)
                {
                    Console.WriteLine(" Done.");
                }
                else
                {
                    Console.WriteLine($"Downloaded {fileName}");
                }
            }

            if (performParallel)
            {
                await Task.WhenAll(candidates.
                    Select(candidate => DownloadAsync(
                        httpAccessor,
                        candidate.fileName, candidate.downloadUrl, candidate.packageBasePath,
                        performParallel)));
            }
            else
            {
                foreach (var candidate in candidates)
                {
                    await DownloadAsync(
                        httpAccessor,
                        candidate.fileName, candidate.downloadUrl, candidate.packageBasePath,
                        performParallel);
                }
            }
        }
        else
        {
            Console.WriteLine("Invalid service endpoint descriptors.");
            Console.WriteLine("Perhaps it's not the correct endpoint?");
        }

        Console.WriteLine();
        Console.WriteLine("Exited.");
    }
}
