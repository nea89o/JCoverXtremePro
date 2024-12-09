using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.JCoverXtremePro;

public class ImageProvider
    : IRemoteImageProvider, IHasOrder
{
    private ILogger _logger;


    public ImageProvider(ILogger<ImageProvider> logger)
    {
        _logger = logger;
    }

    public bool Supports(BaseItem item)
    {
        return item is Movie;
    }

    public string Name => "Mediux";

    public IEnumerable<ImageType> GetSupportedImages(BaseItem item)
    {
        return new List<ImageType>
        {
            ImageType.Primary,
            ImageType.Backdrop,
        };
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        var movieId = item.GetProviderId(MetadataProvider.Tmdb);
        var movieJson =
            await MediuxDownloader.instance.GetMediuxMetadata("https://mediux.pro/movies/" + movieId)
                .ConfigureAwait(false);
        var movieData = JsonSerializer.Deserialize<POJO.MovieData>(movieJson as JsonObject);
        List<RemoteImageInfo> images = new();
        foreach (var set in movieData.allSets)
        {
            foreach (var file in set.files)
            {
                var ft = file.JellyFinFileType();
                if (ft == null)
                {
                    continue;
                }

                if (!file.title.Contains(movieData.movie.title, StringComparison.InvariantCulture))
                {
                    continue;
                }

                var imageInfo = new RemoteImageInfo
                {
                    Url = file.downloadUrl,
                    ProviderName = set.user_created.username + " (via Mediux)",
                    ThumbnailUrl = file.downloadUrl, // TODO: use generated thumbnails from /_next/image?url=
                    Language = "en",
                    RatingType = RatingType.Likes,
                    Type = ft.Value
                };
                images.Add(imageInfo);
            }
        }

        return images;
    }

    public int Order => 0;

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return MediuxDownloader.instance.DownloadFile(url);
    }
}