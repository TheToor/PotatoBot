using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Release
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class ReleaseContainer
    {
        [XmlAttribute(AttributeName = "size")]
        public ulong Size { get; set; }
        [XmlAttribute(AttributeName = "canInstall")]
        public bool CanInstall { get; set; }
        [XmlAttribute(AttributeName = "checkedAt")]
        public ulong CheckedAt { get; set; }
        [XmlAttribute(AttributeName = "downloadURL")]
        public string DownloadUrl { get; set; }
        // Unknown
        [XmlAttribute(AttributeName = "status")]
        public byte Status { get; set; }

        [XmlElement(ElementName = "Release")]
        public Release Release { get; set; }
    }
}
