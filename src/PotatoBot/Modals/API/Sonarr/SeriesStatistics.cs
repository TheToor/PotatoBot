﻿using System;

namespace PotatoBot.Modals.API.Sonarr
{
    public class SeriesStatistics : MediaStatistics
    {
        public int EpisodeCount { get; set; }
        public int EpsideFileCount { get; set; }
        public float PercentOfEpisodes { get; set; }
        public DateTime PreviousAiring { get; set; }
        public int TotalEpisodeCount { get; set; }
    }
}