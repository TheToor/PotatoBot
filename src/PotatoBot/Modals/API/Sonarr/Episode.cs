using System;

namespace PotatoBot.Modals.API.Sonarr
{
    public class Episode
    {
        public int Id { get; set; }
        public uint SeriesId { get; set; }

        public uint EpisodeFileId { get; set; }
        public uint SeasonNumber { get; set; }
        public uint EpisodeNumber { get; set; }
        public string Title { get; set; }
        public DateTime AirDate { get; set; }
        public DateTime AirDateUtc { get; set; }
        public bool HasFile { get; set; }
        public bool Monitored { get; set; }
        public uint AbsoluteEpisodeNumber { get; set; }
        public bool UnverifiedSceneNumbering { get; set; }

        private Series _series;
        public Series Series
        {
            get
            {
                if (_series == null)
                {
                    _series = Program.ServiceManager.Sonarr.GetSeriesInfo(SeriesId);
                }
                return _series;
            }
            set
            {
                _series = value;
            }
        }
    }
}
