using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Jellyfin.Api;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JCoverXtremePro.Api;

[ApiController]
[Route("JCoverXtreme")]
// [Authorize(Policy = "RequiresElevation")]
public class JCoverSharedController : BaseJellyfinApiController
{
    private readonly IProviderManager _providerManager;
    private readonly ILibraryManager _libraryManager;

    /// <summary>
    /// This key is appended to image URLs to inform the frontend about the presence of a potential mass-download.
    /// Keep in sync with coverscript.js#URL_META_KEY.
    /// </summary>
    public static string URL_META_KEY = "JCoverXtremeProMeta";

    public JCoverSharedController(
        IProviderManager providerManager,
        IServerApplicationPaths applicationPaths,
        ILibraryManager libraryManager)
    {
        _providerManager = providerManager;
        _libraryManager = libraryManager;
    }

    public static string AppendUrlMeta(string baseUrl, string key, string value)
    {
        return baseUrl + (baseUrl.Contains('?', StringComparison.InvariantCulture) ? "&" : "?") +
               HttpUtility.UrlEncode(key) + "=" + HttpUtility.UrlEncode(value);
    }

    public class SetMeta
    {
        public Guid seriesId { get; set; }
        public string setId { get; set; }
    }

    public static string PackSetInfo(string baseUrl, Series series, POJO.Set set)
    {
        return AppendUrlMeta(
            baseUrl,
            URL_META_KEY,
            JsonSerializer.Serialize(new SetMeta
            {
                setId = set.id,
                seriesId = series.Id
            }));
    }

    [HttpPost("DownloadSeries")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DownloadEntireSeriesImages(
        [FromBody, Required] JsonObject setMeta
    )
    {
        // TODO: handle missing fields, local seasons missing, series missing, etc.
        var setMetaObj = JsonSerializer.Deserialize<SetMeta>(setMeta);
        var series = _libraryManager.GetItemById(setMetaObj.seriesId) as Series;
        var jsonMeta = await MediuxDownloader.instance.GetMediuxMetadata($"https://mediux.pro/sets/{setMetaObj.setId}")
            .ConfigureAwait(false);
        var set = JsonSerializer.Deserialize<POJO.SetData>(jsonMeta).set;
        Dictionary<string, (int, int)> lut = new();
        foreach (var showSeason in set.show.seasons)
        {
            foreach (var showEpisode in showSeason.episodes)
            {
                lut[showEpisode.id] = (showSeason.season_number, showEpisode.episode_number);
            }
        }

        Dictionary<(int, int), POJO.File> files = new();
        foreach (var file in set.files)
        {
            var episode = file.episode_id;
            if (episode == null)
            {
                continue;
            }

            var tup = lut[episode.id];
            files[tup] = file;
        }

        foreach (var item in series.GetSeasons(null, new DtoOptions(true)))
        {
            var season = item as Season;
            var seasonNumber = season.GetLookupInfo().IndexNumber.Value;
            Plugin.Logger.LogInformation($"Season id: {seasonNumber}:");
            foreach (var itemAgain in season.GetEpisodes())
            {
                var episode = itemAgain as Episode;
                var episodeNumber = episode.GetLookupInfo().IndexNumber.Value;
                Plugin.Logger.LogInformation($" * Episode id: {episodeNumber} {episode.Name}");
                POJO.File file;
                if (files.TryGetValue((seasonNumber, episodeNumber), out file))
                {
                    Plugin.Logger.LogInformation($"   Found cover: {file.downloadUrl}");
                    //         await _providerManager.SaveImage(item, imageUrl, type, null, CancellationToken.None)
                    //     .ConfigureAwait(false);

                    // await item.UpdateToRepositoryAsync(ItemUpdateType.ImageUpdate, CancellationToken.None).ConfigureAwait(false);
                    await _providerManager.SaveImage(
                        episode, file.downloadUrl,
                        // Note: this needs to be updated if SeriesImageProvider ever supports more image types
                        ImageType.Primary,
                        null,
                        CancellationToken.None
                    ).ConfigureAwait(false);

                    await episode.UpdateToRepositoryAsync(ItemUpdateType.ImageUpdate, CancellationToken.None)
                        .ConfigureAwait(false);
                }
            }
        }

        return Empty;
    }
}