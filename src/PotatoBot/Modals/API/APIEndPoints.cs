namespace PotatoBot.Modals.API
{
    public static class APIEndPoints
    {
        public const string SystemStatus = "system/status";
        public const string Calendar = "calendar";
        public const string QualityProfile = "qualityprofile";
        public const string LanguageProfile = "languageprofile";
        public const string Queue = "queue";

        public static class Radarr
        {
            public const string Profile = "profile";
            public const string Movie = "movie";
            public const string Lookup = "movie/lookup";
        }

        public static class Sonarr
        {
            public const string Series = "series";
            public const string Lookup = "series/lookup";

            public const string Episode = "episode/{0}";
        }

        public static class Lidarr
        {
            public const string Artist = "artist";
            public const string Lookup = "artist/lookup";

            public const string Album = "album/{0}";
        }

        public static class Plex
        {
            public const string TokenGeneration = "https://plex.tv/users/sign_in.json";

            public const string Capabilities = "";

            public const string Library = "library/sections";
            public const string LibraryUpdate = "library/sections/{0}/refresh";
            public const string LibraryRefresh = "library/sections/{0}/refresh?force=1";
        }
    }
}
