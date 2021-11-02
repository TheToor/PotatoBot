namespace PotatoBot.WebHook.Modals.Plex
{
	public class Metadata
	{
		public string Guid { get; set; }
		public string Key { get; set; }
		public uint Index { get; set; }
		public string Title { get; set; }
		public string Summary { get; set; }
		public MetaDataType Type { get; set; }
		public string Thumb { get; set; }
		public string Art { get; set; }
		public string RatingKey { get; set; }
		public uint RatingCount { get; set; }

		public string ParentKey { get; set; }
		public string ParentTitle { get; set; }
		public string ParentThumb { get; set; }
		public uint ParentIndex { get; set; }
		public string ParentRatingKey { get; set; }

		public string GrandParentKey { get; set; }
		public string GrandParentTitle { get; set; }
		public string GrandParentThumb { get; set; }
		public string GrandParentArt { get; set; }
		public string GrandParentRatingKey { get; set; }

		public string LibrarySectionType { get; set; }
		public uint LibrarySectionId { get; set; }

		public ulong AddedAt { get; set; }
		public ulong UpdatedAt { get; set; }
	}
}
