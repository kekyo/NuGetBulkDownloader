# NuGetBulkDownloader

----

## What is this?

Useful should to download all nuget packages from a nuget repository.
You can use for backup many nuget package purpose.

```bash
C:\>NuGetBulkDownloader.exe https://example.com/nuget/v3/index.json
NuGetBulkDownloader 0.0.1
Copyright (c) Kouji Matsui
https://github.com/kekyo/NuGetBulkDownloader

Fetching nuget service endpoints from: https://example.com/nuget/v3/index.json ... Done.
Found 110 valid package entries.
Downloading Epoxy.Maui.0.18.1.nupkg ... Done.
Downloading Epoxy.Maui.0.18.0.nupkg ... Done.
Downloading Epoxy.Wpf.0.17.0.nupkg ... Done.
Downloading Epoxy.Wpf.0.16.1.nupkg ... Done.
Downloading Epoxy.Avalonia.0.15.2.nupkg ... Done.
Downloading Epoxy.Avalonia.0.15.0.nupkg ... Done.
  :
  :
```

Downloaded files are placed into `packages/{package id}/{package id}.{package version}.nupkg`.
You can change storing base path with a option below.

## Usage

```bash
C:\>NuGetBulkDownloader.exe
NuGetBulkDownloader 0.0.1
Copyright (c) Kouji Matsui
https://github.com/kekyo/NuGetBulkDownloader

usage: NuGetBulkDownloader [options] <nuget endpoint url>
  -i                         Include prerelease packages
  -p                         Perform parallel download
      --basePath=VALUE       Store packages into this directory
      --userName=VALUE       Basic authentication user name
      --password=VALUE       Basic authentication password
```

----

## (Garbage collection point)

(Maybe it is a fiction)

A source code leak from GitHub occurred in another department.
My team had no problems, and we wouldn't make such a mess.
But I had to salvage all the packages in my work-related private NuGet repository.
So I made it in about 3 hours in a rage.

I have to have a nice meal today...

----

## License

Apache-v2.
