﻿namespace PotatoBot.Webhook.Modals.Radarr
{
    public class Test : RequestBase
    {
        public Movie Movie { get; set; }
        public RemoteMovie RemoteMovie { get; set; }
        public Release Release { get; set; }
    }
}
