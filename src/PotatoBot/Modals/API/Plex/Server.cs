using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex
{
    [XmlRoot(ElementName = "Server")]
    public class Server
    {
        [XmlElement(ElementName = "Section")]
        public List<Section> Section { get; set; }

        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "address")]
        public string Address { get; set; }

        [XmlAttribute(AttributeName = "port")]
        public int Port { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "scheme")]
        public string Scheme { get; set; }

        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }

        [XmlAttribute(AttributeName = "localAddresses")]
        public string LocalAddresses { get; set; }

        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [XmlAttribute(AttributeName = "createdAt")]
        public int CreatedAt { get; set; }

        [XmlAttribute(AttributeName = "updatedAt")]
        public int UpdatedAt { get; set; }

        [XmlAttribute(AttributeName = "owned")]
        public int Owned { get; set; }

        [XmlAttribute(AttributeName = "synced")]
        public int Synced { get; set; }
    }
}
