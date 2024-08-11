using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JCoverXtremePro;

public class MediuxDownloader
{
    public static MediuxDownloader instance;

    private Regex contentRegex = new(@"<script[^>]*>self\.__next_f\.push(.*?)</script>");
    private readonly HttpClient httpClientFactory;
    private string sentinel = "date";

    public MediuxDownloader(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory.CreateClient("MediuxDownloader");
    }

    private List<JsonNode> ExtractJsonNodes(string httpText)
    {
        List<JsonNode> list = new();
        foreach (Match match in contentRegex.Matches(httpText))
        {
            var pushArg = match.Groups[1].Value;
            var strippedString = StripPushArg(pushArg);
            if (!strippedString.Contains(sentinel))
            {
                Plugin.Logger.LogTrace("Ignoring chunk without sentinel {Sentinel}: {Chunk}", sentinel, strippedString);
                continue;
            }

            list.Add(ParseStrippedJsonChunk(strippedString));
        }

        if (list.Count != 1)
        {
            Plugin.Logger.LogError("Found too many or too few chunks: {0}", list);
        }

        return list;
    }

    private JsonNode ParseStrippedJsonChunk(string text)
    {
        return JsonSerializer.Deserialize<JsonArray>(text.Substring(text.IndexOf(':') + 1))[3];
    }

    private string StripPushArg(string text)
    {
        var stringStart = text.IndexOf('"');
        var stringEnd = text.LastIndexOf('"');
        if (stringStart == stringEnd || stringStart == -1)
        {
            return "";
        }

        // TODO: 1 is regular data, 3 is base64 partial data
        return JsonSerializer.Deserialize<string>(text.Substring(stringStart, stringEnd + 1 - stringStart)) ?? "";
    }

    private async Task<string> GetString(string url)
    {
        return await (await httpClientFactory.GetAsync(url).ConfigureAwait(false))
            .Content.ReadAsStringAsync().ConfigureAwait(false);
    }

    private ConcurrentDictionary<string, SemaphoreSlim> cacheLock = new();
    private ConcurrentDictionary<string, JsonNode> cache = new();

    public async Task<JsonNode> GetMediuxMetadata(string url)
    {
        var semaphore = cacheLock.GetOrAdd(url, ignored => new SemaphoreSlim(1));
        try
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            if (cache.TryGetValue(url, out var data))
            {
                Plugin.Logger.LogInformation("Loading cached data from {Url}", url);
                return data;
            }

            Plugin.Logger.LogInformation("Loading data from {Url}", url);
            var text = await GetString(url).ConfigureAwait(false);
            var node = ExtractJsonNodes(text).First();
            cache[url] = node;
            return node;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<HttpResponseMessage> DownloadFile(string url)
    {
        return await httpClientFactory.GetAsync(url).ConfigureAwait(false);
    }
}