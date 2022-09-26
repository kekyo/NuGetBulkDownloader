// NuGetBulkDownloader
// Copyright (c) Kouji Matsui.
// License: Under Apache-v2
// https://github.com/kekyo/NuGetBulkDownloader

using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NuGetBulkDownloader;

internal abstract class OData
{
    [JsonProperty("@id")]
    public Uri? _id { get; set; }
    [JsonProperty("@type")]
    public string? _type { get; set; }
}

internal sealed class Resource : OData
{
    public string? comment { get; set; }
}

internal sealed class ServiceIndex : OData
{
    public string? version { get; set; }
    public Resource[]? resources { get; set; }
}

internal sealed class SearchQueryVersion : OData
{
    public string? version { get; set; }
    public int downloads { get; set; }
}

internal sealed class SearchQueryResult : OData
{
    public string? id { get; set; }
    public string? description { get; set; }
    public SearchQueryVersion[]? versions { get; set; }
    public string? title { get; set; }
    public int totalDownloads { get; set; }
}

internal sealed class SearchQueryResponse : OData
{
    public int totalHits { get; set; }
    public SearchQueryResult[]? data { get; set; }
}

///////////////////////////////////////////////////////////////////

internal sealed class HttpAccessor
{
    private static readonly HttpClient httpClient = new();

    private readonly string? userName;
    private readonly string? password;

    public HttpAccessor(string? userName, string? password)
    {
        this.userName = userName;
        this.password = password;
    }

    public async Task<HttpResponseMessage> RequestQueryAsync(string url)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri(url),
        };

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{this.userName}:{this.password}")));

        var response = await httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            return response;
        }
        else
        {
            throw new Exception($"{response.StatusCode}: {response.ReasonPhrase}");
        }
    }

    public async Task<T> QueryAsync<T>(string url)
    {
        using var response = await RequestQueryAsync(url);
        using var stream = await response.Content.ReadAsStreamAsync();

        var tr = new StreamReader(stream);
        var jsonString = await tr.ReadToEndAsync();

        return JsonConvert.DeserializeObject<T>(jsonString)!;
    }
}
