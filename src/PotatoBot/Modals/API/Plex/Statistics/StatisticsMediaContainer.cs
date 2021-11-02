using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Statistics
{
	[XmlRoot(ElementName = "MediaContainer")]
	public class StatisticsMediaContainer
	{
		[XmlElement(ElementName = "Account")]
		public List<StatisticAccount> Accounts { get; set; } = new List<StatisticAccount>();
		[XmlElement(ElementName = "Device")]
		public List<StatisticDevice> Devices { get; set; } = new List<StatisticDevice>();
		[XmlElement(ElementName = "StatisticsMedia")]
		public List<StatisticsMedia> StatisticsMedia { get; set; } = new List<StatisticsMedia>();

		[XmlAttribute(AttributeName = "size")]
		public string Size { get; set; }
	}
}
