﻿using PotatoBot.Controllers;
using PotatoBot.Model.API;

namespace PotatoBot.Model.API.Radarr
{
    public class RadarrQueueItem : QueueItem
    {
        public Movie Movie { get; set; }

        public RadarrQueueItem(APIBase api) : base(api) { }

        public override string GetQueueTitle()
        {
            return Movie?.Title ?? Title ?? "UNKNOWN";
        }
    }
}
