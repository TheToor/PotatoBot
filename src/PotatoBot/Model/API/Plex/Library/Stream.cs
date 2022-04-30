using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Library
{
    [XmlRoot(ElementName = "Stream")]
    public class Stream
    {
        [XmlAttribute(AttributeName = "id")]
        public int Id { get; set; }

        [XmlAttribute(AttributeName = "streamType")]
        public int StreamType { get; set; }

        [XmlAttribute(AttributeName = "default")]
        public int Default { get; set; }

        [XmlAttribute(AttributeName = "codec")]
        public string Codec { get; set; }

        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "bitrate")]
        public int Bitrate { get; set; }

        [XmlAttribute(AttributeName = "language")]
        public string Language { get; set; }

        [XmlAttribute(AttributeName = "languageTag")]
        public string LanguageTag { get; set; }

        [XmlAttribute(AttributeName = "languageCode")]
        public string LanguageCode { get; set; }

        [XmlAttribute(AttributeName = "bitDepth")]
        public int BitDepth { get; set; }

        [XmlAttribute(AttributeName = "chromaLocation")]
        public string ChromaLocation { get; set; }

        [XmlAttribute(AttributeName = "chromaSubsampling")]
        public string ChromaSubsampling { get; set; }

        [XmlAttribute(AttributeName = "codedHeight")]
        public int CodedHeight { get; set; }

        [XmlAttribute(AttributeName = "codedWidth")]
        public int CodedWidth { get; set; }

        [XmlAttribute(AttributeName = "colorPrimaries")]
        public string ColorPrimaries { get; set; }

        [XmlAttribute(AttributeName = "colorRange")]
        public string ColorRange { get; set; }

        [XmlAttribute(AttributeName = "colorSpace")]
        public string ColorSpace { get; set; }

        [XmlAttribute(AttributeName = "colorTrc")]
        public string ColorTrc { get; set; }

        [XmlAttribute(AttributeName = "frameRate")]
        public double FrameRate { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }

        [XmlAttribute(AttributeName = "level")]
        public int Level { get; set; }

        [XmlAttribute(AttributeName = "profile")]
        public string Profile { get; set; }

        [XmlAttribute(AttributeName = "refFrames")]
        public int RefFrames { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }

        [XmlAttribute(AttributeName = "displayTitle")]
        public string DisplayTitle { get; set; }

        [XmlAttribute(AttributeName = "extendedDisplayTitle")]
        public string ExtendedDisplayTitle { get; set; }

        [XmlAttribute(AttributeName = "channels")]
        public int Channels { get; set; }

        [XmlAttribute(AttributeName = "audioChannelLayout")]
        public string AudioChannelLayout { get; set; }

        [XmlAttribute(AttributeName = "samplingRate")]
        public int SamplingRate { get; set; }

        [XmlAttribute(AttributeName = "selected")]
        public int Selected { get; set; }

        [XmlAttribute(AttributeName = "forced")]
        public int Forced { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }
}
