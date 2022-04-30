using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Statistics
{
    [XmlRoot(ElementName = "Device")]
    public class StatisticDevice
    {
        [XmlAttribute(AttributeName = "id")]
        public ulong Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }
        [XmlAttribute(AttributeName = "createdAt")]
        public ulong CreatedAt { get; set; }
    }
}
