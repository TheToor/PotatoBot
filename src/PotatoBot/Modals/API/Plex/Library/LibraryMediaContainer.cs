using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Library
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class LibraryMediaContainer
    {

        [XmlElement(ElementName = "Video")]
        public List<Video> Video { get; set; }

        [XmlElement(ElementName = "Directory")]
        public List<Directory> Directory { get; set; }

        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }

        [XmlAttribute(AttributeName = "totalSize")]
        public int TotalSize { get; set; }

        [XmlAttribute(AttributeName = "allowSync")]
        public int AllowSync { get; set; }

        [XmlAttribute(AttributeName = "identifier")]
        public string Identifier { get; set; }

        [XmlAttribute(AttributeName = "offset")]
        public int Offset { get; set; }
    }
}
