using System;

namespace PotatoBot.Modals.API
{
    public abstract class QueueItem
    {
        public uint Id { get; set; }

        /// <summary>
        /// The Name of the download not of the Media item (Series, Movie, Artist)
        /// </summary>
        public string Title { get; set; }

        public string DownloadClient { get; set; }
        public string DownloadId { get; set; } 
        public string ErrorMessage { get; set; }
        public DateTime estimatedCompletionTime { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }
        public string Protocol { get; set; }
        public ulong Size { get; set; }
        public ulong SizeLeft { get; set; }
        // This should be an enum
        public string Status { get; set; }
        public string[] StatusMessages { get; set; }
        public string TimeLeft { get; set; }
        public string TrackedDownloadState { get; set; }
        public string TrackedDownloadStatus { get; set; }

        public abstract string GetQueueTitle();
    }
}
