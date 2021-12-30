using Newtonsoft.Json;

namespace PotatoBot.Modals.API.SABnzbd
{
    public class DynamicRoot<T>
	{
		[JsonProperty("status")]
		public T Status { get; set; }

		[JsonProperty("queue")]
		public T Queue { get; set; }
	}
}
