using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JCoverXtremePro;

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
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
            ImageType.Primary
        ];
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        // TODO: hadnle episodes
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
        Plugin.Logger.LogInformation("JSON: " + metadata);
        return [];
    }

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return MediuxDownloader.instance.DownloadFile(url);
    }

    public int Order => 0;
}