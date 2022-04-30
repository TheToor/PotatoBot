using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Library
{
    [XmlRoot(ElementName = "Role")]
    public class RolePerson : Item
    {
        [XmlAttribute(AttributeName = "role")]
        public string Role { get; set; }

        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }
    }
}
