using PotatoBot.Modals.API;
using System.Collections.Generic;

namespace PotatoBot.Modals
{
	public interface IServarr
	{
		ServarrType Type { get; }

		IEnumerable<IServarrItem> GetAll();
		IEnumerable<IServarrItem> Search(string name);
		AddResult Add(IServarrItem item);
		List<QueueItem> GetQueue();
	}
}
