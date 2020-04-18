namespace PotatoBot.Modals.API
{
    public class SystemStatus
    {
        public string Version { get; set; }
        public string BuildTime { get; set; }
        public bool IsDebug { get; set; }
        public bool IsProduction { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsUserInteractive { get; set; }
        public string StartupPath { get; set; }
        public string AppData { get; set; }
        public string OSVersion { get; set; }
        public bool IsMono { get; set; }
        public bool IsLinux { get; set; }
        public bool IsWindows { get; set; }
        public string Branch { get; set; }
        public string Authentication { get; set; }
    }
}
