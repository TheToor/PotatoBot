using PotatoBot.Modals.API;
using System.Collections.Generic;

namespace PotatoBot.Modals
{
    public interface IServarr
    {
        string Name { get; }
        ServarrType Type { get; }

        IEnumerable<IServarrItem> GetAll();
        IEnumerable<IServarrItem> Search(string name);
        IServarrItem GetById(ulong id);

        AddResult Add(IServarrItem item);
        List<QueueItem> GetQueue();
    }
}
