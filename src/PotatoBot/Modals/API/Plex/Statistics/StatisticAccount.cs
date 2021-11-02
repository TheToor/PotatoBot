using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Statistics
{
	[XmlRoot(ElementName = "Account")]
	public class StatisticAccount
	{
		[XmlAttribute(AttributeName = "id")]
		public ulong Id { get; set; }
		[XmlAttribute(AttributeName = "key")]
		public string Key { get; set; }
		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }
		[XmlAttribute(AttributeName = "defaultAudioLanguage")]
		public string DefaultAudioLanguage { get; set; }
		[XmlAttribute(AttributeName = "autoSelectAudio")]
		public bool AutoSelectAudio { get; set; }
		[XmlAttribute(AttributeName = "defaultSubtitleLanguage")]
		public string DefaultSubtitleLanguage { get; set; }
		[XmlAttribute(AttributeName = "subtitleMode")]
		public uint SubtitleMode { get; set; }
		[XmlAttribute(AttributeName = "thumb")]
		public string Thumb { get; set; }
	}
}
