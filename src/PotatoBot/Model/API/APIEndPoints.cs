﻿namespace PotatoBot.Model.API
{
    public static class APIEndPoints
    {
        public const string SystemStatus = "system/status";
        public const string Calendar = "calendar";
        public const string QualityProfile = "qualityprofile";
        public const string LanguageProfile = "languageprofile";
        public const string Queue = "queue";
        public const string DiskSpace = "diskspace";

        public static class RadarrEndpoints
        {
            public const string Profile = "profile";
            public const string Movie = "movie";
            public const string Lookup = "movie/lookup";

            public const string ImportList = "importlist/movie";
        }

        public static class SonarrEndpoints
        {
            public const string Series = "series";
            public const string Lookup = "series/lookup";

            public const string Episode = "episode/{0}";
        }

        public static class LidarrEndpoints
        {
            public const string Artist = "artist";
            public const string Lookup = "artist/lookup";

            public const string Album = "album/{0}";
        }

        public static class PlexEndpoints
        {
            public const string TokenGeneration = "https://plex.tv/users/sign_in.json";

            public const string Capabilities = "";

            public const string Library = "library/sections";
            public const string LibraryUpdate = "library/sections/{0}/refresh";
            public const string LibraryRefresh = "library/sections/{0}/refresh?force=1";
            public const string AllLibraryRecentlyAdded = "library/recentlyAdded";
            public const string RecentlyAdded = "hubs/home/recentlyAdded?type={0}&sectionID={1}&contentDirectoryID={1}&X-Plex-Container-Start=0&X-Plex-Container-Size={2}";

            public const string MediaStatistics = "statistics/media?timespan=1&at>=0";

            public const string UpdateStatus = "updater/status";

            public const string MediaProviders = "media/providers";

            // V1 API
            public const string Invite = "https://plex.tv/api/servers/{0}/shared_servers{1}";
            // V2 API
            //public const string Invite = "https://plex.tv/api/v2/shared_servers{1}";
            public const string Section = "https://plex.tv/api/servers/{0}";
        }
    }
}
