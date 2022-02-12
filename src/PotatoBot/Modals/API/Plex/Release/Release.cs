using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Release
{
    [XmlRoot(ElementName = "Release")]
    public class Release
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }
        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }
        [XmlAttribute(AttributeName = "added")]
        public string Added { get; set; }
        [XmlAttribute(AttributeName = "fixed")]
        public string Fixed { get; set; }
        [XmlAttribute(AttributeName = "downloadURL")]
        public string DownloadUrl { get; set; }
        [XmlAttribute(AttributeName = "state")]
        // Should probably be an ENUM
        public string State { get; set; }
    }
}
