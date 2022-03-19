using System.Collections.Generic;

namespace PotatoBot.Modals
{
    public class PageResult<T>
    {
        public int Start { get; set; }
        public int End { get; set; }

        public bool NextPossible { get; set; }
        public bool PreviousPossible { get; set; }

        public List<T> Items { get; set; } = new List<T>();
    }
}
