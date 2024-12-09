using System.Linq;

namespace Jellyfin.Plugin.JCoverXtremePro;

using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.JCoverXtremePro.Api;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;

public class SeriesImageProvider
    : IRemoteImageProvider, IHasOrder
{
    public bool Supports(BaseItem item)
    {
        return item is Episode or Series;
    }

    public string Name => "Mediux Series";

    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return
        [
            // Note: update JCoverSharedController if more image types are supported
            ImageType.Primary
        ];
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        // TODO: handle specific episodes directly
        if (item is Series series)
        {
            return await HandleSeries(series, cancellationToken);
        }

        return [];
    }

    public async Task<IEnumerable<RemoteImageInfo>> HandleSeries(Series series, CancellationToken token)
    {
        var tmdbId = series.GetProviderId(MetadataProvider.Tmdb);
        if (tmdbId == null)
        {
            return []; // TODO: handle missing id
        }

        var metadata = await MediuxDownloader.instance.GetMediuxMetadata("https://mediux.pro/shows/" + tmdbId)
            .ConfigureAwait(false);
        var show = JsonSerializer.Deserialize<POJO.ShowData>(metadata as JsonObject)!;

        return from set in show.sets
            let representativeImage = set.files.Find(it => it.fileType is "poster" or "title_card")!
            let enrichedUrl = JCoverSharedController.PackSetInfo(representativeImage.downloadUrl, series, set)
            select new RemoteImageInfo
            {
                Url = enrichedUrl,
                ProviderName = set.user_created.username + " (via Mediux)",
                ThumbnailUrl = enrichedUrl, // TODO: use generated thumbnails from /_next/image?url=
                Language = "en",
                RatingType = RatingType.Likes,
                Type = representativeImage.JellyFinFileType().Value
            };
    }

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return MediuxDownloader.instance.DownloadFile(url);
    }

    public int Order => 0;
}