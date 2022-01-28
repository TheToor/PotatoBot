using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Providers
{
    [XmlRoot(ElementName = "MediaContainer")]
    public class ProviderMediaContainer
    {
        [XmlAttribute(AttributeName = "size")]
        public int Size { get; set; }

        [XmlAttribute(AttributeName = "allowCameraUpload")]
        public int AllowCameraUpload { get; set; }

        [XmlAttribute(AttributeName = "allowChannelAccess")]
        public int AllowChannelAccess { get; set; }

        [XmlAttribute(AttributeName = "allowSharing")]
        public int AllowSharing { get; set; }

        [XmlAttribute(AttributeName = "allowSync")]
        public int AllowSync { get; set; }

        [XmlAttribute(AttributeName = "allowTuners")]
        public int AllowTuners { get; set; }

        [XmlAttribute(AttributeName = "backgroundProcessing")]
        public int BackgroundProcessing { get; set; }

        [XmlAttribute(AttributeName = "certificate")]
        public int Certificate { get; set; }

        [XmlAttribute(AttributeName = "companionProxy")]
        public int CompanionProxy { get; set; }

        [XmlAttribute(AttributeName = "countryCode")]
        public string CountryCode { get; set; }

        [XmlAttribute(AttributeName = "diagnostics")]
        public string Diagnostics { get; set; }

        [XmlAttribute(AttributeName = "eventStream")]
        public int EventStream { get; set; }

        [XmlAttribute(AttributeName = "friendlyName")]
        public string FriendlyName { get; set; }

        [XmlAttribute(AttributeName = "livetv")]
        public int Livetv { get; set; }

        [XmlAttribute(AttributeName = "machineIdentifier")]
        public string MachineIdentifier { get; set; }

        [XmlAttribute(AttributeName = "musicAnalysis")]
        public int MusicAnalysis { get; set; }

        [XmlAttribute(AttributeName = "myPlex")]
        public int MyPlex { get; set; }

        [XmlAttribute(AttributeName = "myPlexMappingState")]
        public string MyPlexMappingState { get; set; }

        [XmlAttribute(AttributeName = "myPlexSigninState")]
        public string MyPlexSigninState { get; set; }

        [XmlAttribute(AttributeName = "myPlexSubscription")]
        public int MyPlexSubscription { get; set; }

        [XmlAttribute(AttributeName = "myPlexUsername")]
        public string MyPlexUsername { get; set; }

        [XmlAttribute(AttributeName = "offlineTranscode")]
        public int OfflineTranscode { get; set; }

        [XmlAttribute(AttributeName = "ownerFeatures")]
        public string OwnerFeatures { get; set; }

        [XmlAttribute(AttributeName = "photoAutoTag")]
        public int PhotoAutoTag { get; set; }

        [XmlAttribute(AttributeName = "platform")]
        public string Platform { get; set; }

        [XmlAttribute(AttributeName = "platformVersion")]
        public string PlatformVersion { get; set; }

        [XmlAttribute(AttributeName = "pluginHost")]
        public int PluginHost { get; set; }

        [XmlAttribute(AttributeName = "pushNotifications")]
        public int PushNotifications { get; set; }

        [XmlAttribute(AttributeName = "readOnlyLibraries")]
        public int ReadOnlyLibraries { get; set; }

        [XmlAttribute(AttributeName = "streamingBrainABRVersion")]
        public int StreamingBrainABRVersion { get; set; }

        [XmlAttribute(AttributeName = "streamingBrainVersion")]
        public int StreamingBrainVersion { get; set; }

        [XmlAttribute(AttributeName = "sync")]
        public int Sync { get; set; }

        [XmlAttribute(AttributeName = "transcoderActiveVideoSessions")]
        public int TranscoderActiveVideoSessions { get; set; }

        [XmlAttribute(AttributeName = "transcoderAudio")]
        public int TranscoderAudio { get; set; }

        [XmlAttribute(AttributeName = "transcoderLyrics")]
        public int TranscoderLyrics { get; set; }

        [XmlAttribute(AttributeName = "transcoderSubtitles")]
        public int TranscoderSubtitles { get; set; }

        [XmlAttribute(AttributeName = "transcoderVideo")]
        public int TranscoderVideo { get; set; }

        [XmlAttribute(AttributeName = "transcoderVideoBitrates")]
        public string TranscoderVideoBitrates { get; set; }

        [XmlAttribute(AttributeName = "transcoderVideoQualities")]
        public string TranscoderVideoQualities { get; set; }

        [XmlAttribute(AttributeName = "transcoderVideoResolutions")]
        public string TranscoderVideoResolutions { get; set; }

        [XmlAttribute(AttributeName = "updatedAt")]
        public int UpdatedAt { get; set; }

        [XmlAttribute(AttributeName = "updater")]
        public int Updater { get; set; }

        [XmlAttribute(AttributeName = "version")]
        public string Version { get; set; }

        [XmlAttribute(AttributeName = "voiceSearch")]
        public int VoiceSearch { get; set; }
    }
}
