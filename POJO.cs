using System;
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

    public class SetData
    {
        public Set set { get; set; }
    }

    public class ShowData
    {
        // public Show show { get; set; }
        public List<Set> sets { get; set; }
    }

    public class Set
    {
        public string id { get; set; }

        public string set_name { get; set; }

        public User user_created { get; set; }
        public List<File> files { get; set; }
        public Show? show { get; set; }
    }

    public class Show
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<Season> seasons { get; set; }
    }

    public class Season
    {
        public string id { get; set; }
        public int season_number { get; set; }
        public string name { get; set; }
        public List<Episode> episodes { get; set; }
    }

    public class Episode
    {
        public int episode_number { get; set; }
        public string episode_name { get; set; }
        public string id { get; set; }
    }

    public class User
    {
        public string username { get; set; }
    }

    public class EpisodeId
    {
        public string id { get; set; }
    }

    public class File
    {
        public string fileType { get; set; }
        public string title { get; set; }
        public string id { get; set; }
        public EpisodeId? episode_id { get; set; }

        public ImageType? JellyFinFileType()
        {
            switch (fileType)
            {
                case "backdrop":
                    return ImageType.Backdrop;
                case "poster":
                case "title_card":
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