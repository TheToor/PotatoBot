using Newtonsoft.Json;
using System.Collections.Generic;

namespace PotatoBot.Modals.API.SABnzbd
{
    public class Server
	{
		[JsonProperty("servertotalconn")]
		public int ServerTotalConnections { get; set; }

		[JsonProperty("serverssl")]
		public int ServerSSL { get; set; }

		[JsonProperty("serveractiveconn")]
		public int ServerActiveConnections { get; set; }

		[JsonProperty("serveroptional")]
		public int ServerOptional { get; set; }

		[JsonProperty("serverconnections")]
		public List<object> ServerConnections { get; set; }

		[JsonProperty("servername")]
		public string ServerName { get; set; }

		[JsonProperty("serveractive")]
		public bool ServerActive { get; set; }

		[JsonProperty("serversslinfo")]
		public string ServerSSLInfo { get; set; }

		[JsonProperty("servererror")]
		public string ServerError { get; set; }

		[JsonProperty("serverpriority")]
		public int ServerPriority { get; set; }
	}
}
