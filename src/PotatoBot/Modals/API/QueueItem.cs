using PotatoBot.Controllers;
using System;
using System.Collections.Generic;

namespace PotatoBot.Modals.API
{
    public abstract class QueueItem
    {
        public uint Id { get; set; }
        public APIBase API { get; set; }

        /// <summary>
        /// The Name of the download not of the Media item (Series, Movie, Artist)
        /// </summary>
        public string Title { get; set; }

        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime EstimatedCompletionTime { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }
        public string Protocol { get; set; }
        public ulong Size { get; set; }
        public ulong SizeLeft { get; set; }
        // This should be an enum
        public string Status { get; set; }
        public string TimeLeft { get; set; }

        public TrackedDownloadState TrackedDownloadState { get; set; }
        public TrackedDownloadStatus TrackedDownloadStatus { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }

        public QueueItem(APIBase api)
        {
            API = api;
        }

        public abstract string GetQueueTitle();
    }
}
