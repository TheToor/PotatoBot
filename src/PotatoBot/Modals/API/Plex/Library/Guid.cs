using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Library
{
    [XmlRoot(ElementName = "Guid")]
    public class Guid
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
    }
}
