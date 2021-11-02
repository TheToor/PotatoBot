namespace PotatoBot.Modals.API
{
	public class DiskSpace
	{
		public string Path { get; set; }
		public string Label { get; set; }
		public ulong FreeSpace { get; set; }
		public ulong TotalSpace { get; set; }
	}
}
