using Newtonsoft.Json;
using System.Collections.Generic;

namespace PotatoBot.Modals.API.SABnzbd
{
    public class SABQueue : ServerStatus
    {
        [JsonProperty("noofslots_total")]
        public int NoofslotsTotal { get; set; }

        [JsonProperty("noofslots")]
        public int Noofslots { get; set; }

        [JsonProperty("finish")]
        public int Finish { get; set; }

        [JsonProperty("speedlimit_abs")]
        public string SpeedlimitAbs { get; set; }

        // We don't care about the slots atm (queue items)
        //[JsonProperty("slots")]
        //public List<Slot> Slots { get; set; }

        [JsonProperty("speed")]
        public string Speed { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("rating_enable")]
        public bool RatingEnable { get; set; }

        [JsonProperty("eta")]
        public string EstimatedRemainingTime { get; set; }

        [JsonProperty("refresh_rate")]
        public string RefreshRate { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("sizeleft")]
        public string SizeLeft { get; set; }

        [JsonProperty("finishaction")]
        public object Finishaction { get; set; }

        [JsonProperty("mbleft")]
        public string MMRemaining { get; set; }

        [JsonProperty("scripts")]
        public List<object> Scripts { get; set; }

        [JsonProperty("categories")]
        public List<string> Categories { get; set; }

        [JsonProperty("timeleft")]
        public string TimeLeft { get; set; }

        [JsonProperty("mb")]
        public string MB { get; set; }

        [JsonProperty("kbpersec")]
        public string KBPerSecond { get; set; }

        [JsonProperty("queue_details")]
        public string QueueDetails { get; set; }
    }
}
