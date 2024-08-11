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
            // ImageType.Backdrop,
        };
    }

    public async Task<IEnumerable<RemoteImageInfo>> GetImages(BaseItem item, CancellationToken cancellationToken)
    {
        var movie = item as Movie;
        var movieId = item.GetProviderId(MetadataProvider.Tmdb);
        var collectionId = item.GetProviderId(MetadataProvider.TmdbCollection);
        _logger.LogInformation(
            $"Help i am stuck in a movie labeling factory and have been taskted to label {movie.Name} " +
            $"({movieId} in collection {collectionId})"
        );
        var movieData =
            await MediuxDownloader.instance.GetMediuxMetadata("https://mediux.pro/movies/" + movieId)
                .ConfigureAwait(false);
        var deserMovieData = JsonSerializer.Deserialize<POJO.MovieData>(movieData as JsonObject);
        _logger.LogInformation("Movie Data: {JsonData}", movieData.ToJsonString());
        _logger.LogInformation("Movie Data Decoded: {Data}", JsonSerializer.Serialize(deserMovieData));
        List<RemoteImageInfo> images = new();
        foreach (var set in deserMovieData.allSets)
        {
            _logger.LogInformation("Set Data: {Name} {Data}", set.set_name, set.files.Count);
            foreach (var file in set.files)
            {
                _logger.LogInformation("Matching file {Name}", JsonSerializer.Serialize(file));
                if (file.fileType != "poster")
                {
                    _logger.LogInformation("Skipping non poster file");
                    continue;
                }

                if (file.title.Contains(deserMovieData.movie.title))
                {
                    _logger.LogInformation("Adding image");
                    var imageInfo = new RemoteImageInfo
                    {
                        Url = file.downloadUrl,
                        ProviderName = Name,
                        ThumbnailUrl = file.downloadUrl,
                        Language = "en",
                        RatingType = RatingType.Likes,
                        Type = ImageType.Primary,
                    };
                    _logger.LogInformation("Constructed image");
                    images.Add(imageInfo);
                    _logger.LogInformation("Appended image");
                }
            }
        }

        _logger.LogInformation("Collected images {0}", images);
        return images;
    }

    public int Order => 0;

    public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
    {
        return MediuxDownloader.instance.DownloadFile(url);
    }
}