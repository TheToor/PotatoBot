using System.Collections.Generic;

namespace PotatoBot.Modals.Settings
{
    public class LettuceEncryptSettings
    {
        public bool AcceptTOS { get; set; }
        public List<string> DomainNames { get; set; }
        public string Email { get; set; }
    }
}
