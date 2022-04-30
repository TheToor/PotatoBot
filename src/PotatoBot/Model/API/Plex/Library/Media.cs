using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Library
{
    [XmlRoot(ElementName = "Media")]
    public class Media
    {
        [XmlElement(ElementName = "Part")]
        public Part Part { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public int Duration { get; set; }

        [XmlAttribute(AttributeName = "bitrate")]
        public int Bitrate { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

        [XmlAttribute(AttributeName = "aspectRatio")]
        public double AspectRatio { get; set; }

        [XmlAttribute(AttributeName = "audioChannels")]
        public int AudioChannels { get; set; }

        [XmlAttribute(AttributeName = "audioCodec")]
        public string AudioCodec { get; set; }

        [XmlAttribute(AttributeName = "videoCodec")]
        public string VideoCodec { get; set; }

        [XmlAttribute(AttributeName = "videoResolution")]
        public string VideoResolution { get; set; }

        [XmlAttribute(AttributeName = "container")]
        public string Container { get; set; }

        [XmlAttribute(AttributeName = "videoFrameRate")]
        public string VideoFrameRate { get; set; }

        [XmlAttribute(AttributeName = "videoProfile")]
        public string VideoProfile { get; set; }
    }
}
