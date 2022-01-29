using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex
{
    [XmlRoot(ElementName = "Section")]
    public class Section
    {

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "key")]
        public int Key { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
}
}
