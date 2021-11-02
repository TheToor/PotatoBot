using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex
{
	[XmlRoot(ElementName = "MediaContainer")]
	public class MediaContainer
	{
		[XmlElement(ElementName = "Directory")]
		public List<Directory> Directory { get; set; }
		[XmlAttribute(AttributeName = "size")]
		public string Size { get; set; }
		[XmlAttribute(AttributeName = "allowMediaDeletion")]
		public string AllowMediaDeletion { get; set; }
		[XmlAttribute(AttributeName = "flashInstalled")]
		public string FlashInstalled { get; set; }
		[XmlAttribute(AttributeName = "friendlyName")]
		public string FriendlyName { get; set; }
		[XmlAttribute(AttributeName = "machineIdentifier")]
		public string MachineIdentifier { get; set; }
		[XmlAttribute(AttributeName = "multiuser")]
		public string Multiuser { get; set; }
		[XmlAttribute(AttributeName = "myPlex")]
		public string MyPlex { get; set; }
		[XmlAttribute(AttributeName = "myPlexMappingState")]
		public string MyPlexMappingState { get; set; }
		[XmlAttribute(AttributeName = "myPlexSigninState")]
		public string MyPlexSigninState { get; set; }
		[XmlAttribute(AttributeName = "myPlexUsername")]
		public string MyPlexUsername { get; set; }
		[XmlAttribute(AttributeName = "platform")]
		public string Platform { get; set; }
		[XmlAttribute(AttributeName = "platformVersion")]
		public string PlatformVersion { get; set; }
		[XmlAttribute(AttributeName = "requestParametersInCookie")]
		public string RequestParametersInCookie { get; set; }
		[XmlAttribute(AttributeName = "silverlightInstalled")]
		public string SilverlightInstalled { get; set; }
		[XmlAttribute(AttributeName = "soundflowerInstalled")]
		public string SoundflowerInstalled { get; set; }
		[XmlAttribute(AttributeName = "sync")]
		public string Sync { get; set; }
		[XmlAttribute(AttributeName = "transcoderActiveVideoSessions")]
		public string TranscoderActiveVideoSessions { get; set; }
		[XmlAttribute(AttributeName = "transcoderAudio")]
		public string TranscoderAudio { get; set; }
		[XmlAttribute(AttributeName = "transcoderVideo")]
		public string TranscoderVideo { get; set; }
		[XmlAttribute(AttributeName = "transcoderVideoBitrates")]
		public string TranscoderVideoBitrates { get; set; }
		[XmlAttribute(AttributeName = "transcoderVideoQualities")]
		public string TranscoderVideoQualities { get; set; }
		[XmlAttribute(AttributeName = "transcoderVideoResolutions")]
		public string TranscoderVideoResolutions { get; set; }
		[XmlAttribute(AttributeName = "updatedAt")]
		public string UpdatedAt { get; set; }
		[XmlAttribute(AttributeName = "version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName = "webkit")]
		public string Webkit { get; set; }

		// Libraries
		[XmlAttribute(AttributeName = "allowSync")]
		public string AllowSync { get; set; }
		[XmlAttribute(AttributeName = "identifier")]
		public string Identifier { get; set; }
		[XmlAttribute(AttributeName = "mediaTagPrefix")]
		public string MediaTagPrefix { get; set; }
		[XmlAttribute(AttributeName = "mediaTagVersion")]
		public string MediaTagVersion { get; set; }
		[XmlAttribute(AttributeName = "title1")]
		public string Title1 { get; set; }
	}
}
