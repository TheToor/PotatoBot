using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Library
{
    [XmlRoot(ElementName = "Part")]
    public class Part
    {
        [XmlElement(ElementName = "Stream")]
        public List<Stream> Stream { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public int Duration { get; set; }

        [XmlAttribute(AttributeName = "file")]
        public string File { get; set; }

        [XmlAttribute(AttributeName = "size")]
        public double Size { get; set; }

        [XmlAttribute(AttributeName = "container")]
        public string Container { get; set; }

        [XmlAttribute(AttributeName = "videoProfile")]
        public string VideoProfile { get; set; }

        [XmlAttribute(AttributeName = "hasThumbnail")]
        public int HasThumbnail { get; set; }
    }

}
