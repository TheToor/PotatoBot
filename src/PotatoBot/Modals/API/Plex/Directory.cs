using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex
{
    [XmlRoot(ElementName = "Directory")]
    public class Directory
    {
        [XmlElement(ElementName = "Location")]
        public Location Location { get; set; }
        [XmlAttribute(AttributeName = "allowSync")]
        public string AllowSync { get; set; }
        [XmlAttribute(AttributeName = "art")]
        public string Art { get; set; }
        [XmlAttribute(AttributeName = "filters")]
        public string Filters { get; set; }
        [XmlAttribute(AttributeName = "refreshing")]
        public string Refreshing { get; set; }
        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "agent")]
        public string Agent { get; set; }
        [XmlAttribute(AttributeName = "scanner")]
        public string Scanner { get; set; }
        [XmlAttribute(AttributeName = "language")]
        public string Language { get; set; }
        [XmlAttribute(AttributeName = "uuid")]
        public string Uuid { get; set; }
        [XmlAttribute(AttributeName = "updatedAt")]
        public string UpdatedAt { get; set; }
        [XmlAttribute(AttributeName = "createdAt")]
        public string CreatedAt { get; set; }

        [XmlAttribute(AttributeName = "count")]
        public int Count { get; set; }
    }
}
