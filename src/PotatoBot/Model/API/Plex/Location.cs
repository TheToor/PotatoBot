using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex
{
    [XmlRoot(ElementName = "Location")]
    public class Location
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }
    }
}
