using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class SectionMediaContainer
    {
        [XmlElement(ElementName = "Server")]
        public Server Server { get; set; }

        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }

        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }

        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }
    }
}
