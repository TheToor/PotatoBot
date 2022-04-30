using System.Net;

namespace PotatoBot.Model
{
    public class AddResult
    {
        public bool Added { get; set; }
        public bool AlreadyAdded { get; set; }
        public HttpStatusCode StatusCode { get; set; }
    }
}
