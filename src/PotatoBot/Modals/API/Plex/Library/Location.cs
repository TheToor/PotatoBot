using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Library
{
    [XmlRoot(ElementName = "Location")]
    public class Location
    {

        [XmlAttribute(AttributeName = "path")]
        public string Path { get; set; }
    }
}
