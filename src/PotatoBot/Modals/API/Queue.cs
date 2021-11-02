using System.Collections.Generic;

namespace PotatoBot.Modals.API
{
	public class Queue<T>
	{
		public uint Page { get; set; }
		public uint PageSize { get; set; }
		public string SortKey { get; set; }
		public string SortDirection { get; set; }
		public uint TotalRecords { get; set; }
		public List<T> Records { get; set; }
	}
}
