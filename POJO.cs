using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MediaBrowser.Model.Entities;

namespace Jellyfin.Plugin.JCoverXtremePro;

public class POJO
{
    public class MovieData
    {
        public Movie movie { get; set; }
        public List<Set> sets { get; set; }
        public List<Set> collectionSets { get; set; }
        [JsonIgnore] public IEnumerable<Set> allSets => sets.Concat(collectionSets);
    }

    public class Set
    {
        public string id { get; set; }

        public string set_name { get; set; }

        public User user_created { get; set; }
        public List<File> files { get; set; }
    }

    public class User
    {
        public string username { get; set; }
    }

    public class File
    {
        public string fileType { get; set; }
        public string title { get; set; }
        public string id { get; set; }

        public ImageType? JellyFinFileType()
        {
            switch (fileType)
            {
                case "backdrop":
                    return ImageType.Backdrop;
                case "poster":
                    return ImageType.Primary;
            }

            return null;
        }


        [JsonIgnore] public string downloadUrl => "https://api.mediux.pro/assets/" + id;
    }

    public class Movie
    {
        public string id { get; set; }
        public string title { get; set; }
        public string tagline { get; set; }
        public string imdb_id { get; set; }
    }
}