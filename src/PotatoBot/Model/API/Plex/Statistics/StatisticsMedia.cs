using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Statistics
{
    [XmlRoot(ElementName = "StatisticsMedia")]
    public class StatisticsMedia
    {
        [XmlAttribute(AttributeName = "accountID")]
        public ulong AccountId { get; set; }
        [XmlAttribute(AttributeName = "deviceID")]
        public ulong DeviceID { get; set; }
        [XmlAttribute(AttributeName = "timespan")]
        public ulong TimeSpan { get; set; }
        [XmlAttribute(AttributeName = "at")]
        public ulong At { get; set; }
        [XmlAttribute(AttributeName = "metadataType")]
        public uint MetaDataType { get; set; }
        [XmlAttribute(AttributeName = "count")]
        public ulong Count { get; set; }
        [XmlAttribute(AttributeName = "duration")]
        public ulong Duration { get; set; }
    }
}
