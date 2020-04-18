﻿using System;
using System.Collections.Generic;

namespace PotatoBot.Modals.API.Sonarr
{
    public class Series
    {
        public ulong Id { get; set; }
        public uint LanguageProfileId { get; set; }
        public uint QualityProfileId { get; set; }

        public string Title { get; set; }

        public DateTime Added { get; set; }
        public string AirTime { get; set; }
        public string Certification { get; set; }
        public string CleanTitle { get; set; }
        public bool Ended { get; set; }
        public DateTime FirstAired { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
        public List<Image> Images { get; set; } = new List<Image>();
        public string ImdbId { get; set; }
        public bool Monitored { get; set; }
        public string Network { get; set; }
        public DateTime NextAiring { get; set; }
        public string Overview { get; set; }
        public string Path { get; set; }
        public DateTime PreviousAiring { get; set; }
        public Rating Ratings { get; set; }
        public int Runtime { get; set; }
        public bool SeasonFolder { get; set; }
        public List<SimpleSeason> Seasons { get; set; }
        // This should be an enum
        public string SeriesType { get; set; }
        public string SortTitle { get; set; }
        public Statistics Statistics { get; set; }
        // This should be an enum
        public string Status { get; set; }
        // To check
        public List<string> Tags { get; set; }
        public string TitleSlug { get; set; }
        public int TvMazeId { get; set; }
        public int TvRageId { get; set; }
        public int TvDbId { get; set; }
        public bool UseSceneNumbering { get; set; }
        public int Year { get; set; }
    }
}
