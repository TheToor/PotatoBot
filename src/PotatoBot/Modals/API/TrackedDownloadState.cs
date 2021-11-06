namespace PotatoBot.Modals.API
{
	// https://github.com/Radarr/Radarr/blob/627ab64fd023269c8bedece61e529329600a3419/src/NzbDrone.Core/Download/TrackedDownloads/TrackedDownload.cs#L37
	public enum TrackedDownloadState
	{
		Downloading,
		ImportPending,
		Importing,
		Imported,
		FailedPending,
		Failed,
		Ignored
	}
}
