using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Library
{
    public abstract class Item
    {
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "filter")]
        public string Filter { get; set; }

        [XmlAttribute(AttributeName = "tag")]
        public string Tag { get; set; }
    }
}
