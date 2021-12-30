using System.Collections.Generic;

namespace PotatoBot.Modals.API
{
    // https://github.com/Radarr/Radarr/blob/627ab64fd023269c8bedece61e529329600a3419/src/NzbDrone.Core/Download/TrackedDownloads/TrackedDownloadStatusMessage.cs
    public class TrackedDownloadStatusMessage
    {
        public string Title { get; set; }
        public List<string> Messages { get; set; }

        public TrackedDownloadStatusMessage(string title, List<string> messages)
        {
            Title = title;
            Messages = messages;
        }

        public TrackedDownloadStatusMessage(string title, string message)
        {
            Title = title;
            Messages = new List<string> { message };
        }

        //Constructor for use when deserializing JSON
        private TrackedDownloadStatusMessage()
        {
        }
    }
}
