using Newtonsoft.Json;
using System.Collections.Generic;

namespace PotatoBot.Modals.API.SABnzbd
{
	public class ServerStatus
	{
		[JsonProperty("pid")]
		public int Pid { get; set; }

		[JsonProperty("cache_size")]
		public string CacheSize { get; set; }

		[JsonProperty("active_lang")]
		public string ActiveLang { get; set; }

		[JsonProperty("servers")]
		public List<Server> Servers { get; set; }

		[JsonProperty("pp_pause_event")]
		public bool PpPauseEvent { get; set; }

		[JsonProperty("session")]
		public string Session { get; set; }

		[JsonProperty("url_base")]
		public string UrlBase { get; set; }

		[JsonProperty("restart_req")]
		public bool RestartReq { get; set; }

		[JsonProperty("power_options")]
		public bool PowerOptions { get; set; }

		[JsonProperty("pystone")]
		public int Pystone { get; set; }

		[JsonProperty("helpuri")]
		public string Helpuri { get; set; }

		[JsonProperty("uptime")]
		public string Uptime { get; set; }

		[JsonProperty("logfile")]
		public string Logfile { get; set; }

		[JsonProperty("downloaddir")]
		public string Downloaddir { get; set; }

		[JsonProperty("my_home")]
		public string MyHome { get; set; }

		[JsonProperty("version")]
		public string Version { get; set; }

		[JsonProperty("completedirspeed")]
		public int Completedirspeed { get; set; }

		[JsonProperty("my_lcldata")]
		public string MyLcldata { get; set; }

		[JsonProperty("color_scheme")]
		public string ColorScheme { get; set; }

		[JsonProperty("new_release")]
		public object NewRelease { get; set; }

		[JsonProperty("nt")]
		public bool Nt { get; set; }

		[JsonProperty("configfn")]
		public string Configfn { get; set; }

		[JsonProperty("folders")]
		public List<object> Folders { get; set; }

		[JsonProperty("have_warnings")]
		public string HaveWarnings { get; set; }

		[JsonProperty("weblogfile")]
		public object Weblogfile { get; set; }

		[JsonProperty("cache_art")]
		public string CacheArt { get; set; }

		[JsonProperty("warnings")]
		public List<object> Warnings { get; set; }



		[JsonProperty("downloaddirspeed")]
		public int DownloadDirectorySpeed { get; set; }

		[JsonProperty("finishaction")]
		public object FinishAction { get; set; }

		[JsonProperty("paused_all")]
		public bool PausedAll { get; set; }

		[JsonProperty("quota")]
		public string Quota { get; set; }

		[JsonProperty("pause_int")]
		public string PauseInt { get; set; }

		[JsonProperty("completedir")]
		public string CompleteDirectory { get; set; }

		[JsonProperty("loadavg")]
		public string LoadAverage { get; set; }

		[JsonProperty("paused")]
		public bool Paused { get; set; }

		[JsonProperty("darwin")]
		public bool Darwin { get; set; }

		[JsonProperty("speedlimit_abs")]
		public string SpeedlimitAbsolute { get; set; }

		[JsonProperty("cpumodel")]
		public string CpuModel { get; set; }

		[JsonProperty("have_quota")]
		public bool HaveQuota { get; set; }

		[JsonProperty("loglevel")]
		public string LogLevel { get; set; }

		[JsonProperty("new_rel_url")]
		public string NewRelUrl { get; set; }

		[JsonProperty("cache_max")]
		public string CacheMax { get; set; }

		[JsonProperty("speedlimit")]
		public string SpeedLimit { get; set; }

		[JsonProperty("webdir")]
		public string Webdir { get; set; }

		[JsonProperty("left_quota")]
		public string LeftQuota { get; set; }

		[JsonProperty("diskspace1")]
		public string Diskspace1 { get; set; }
		[JsonProperty("diskspacetotal1")]
		public string DiskspaceTotal1 { get; set; }
		[JsonProperty("diskspace1_norm")]
		public string Diskspace1Norm { get; set; }


		[JsonProperty("diskspace2")]
		public string Diskspace2 { get; set; }
		[JsonProperty("diskspacetotal2")]
		public string DiskspaceTotal2 { get; set; }
		[JsonProperty("diskspace2_norm")]
		public string Diskspace2Norm { get; set; }
	}
}
