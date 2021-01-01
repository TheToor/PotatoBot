using System;
using System.Collections.Generic;
using System.Linq;

namespace PotatoBot.Extensions
{
    public class PageResult<T>
    {
        public int Start { get; set; }
        public int End { get; set; }

        public bool NextPossible { get; set; }
        public bool PreviousPossible { get; set; }

        public List<T> Items { get; set; } = new List<T>();
    }
    public static class ListExtensions
    {
        public static PageResult<T> TakePaged<T>(this List<T> list, int page, int itemsPerPage)
        {
            if(list.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(list));
            }
            if(page < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), page, "Page needs to be greaten than 0");
            }

            var result = new PageResult<T>();

            result.Start = page * itemsPerPage;
            result.End = result.Start + itemsPerPage;

            if(result.Start >= list.Count)
            {
                // We are over the end so just return an empty List
                
                if(result.Start - itemsPerPage < list.Count)
                {
                    result.PreviousPossible = true;
                }

                result.Start = list.Count;
                result.End = list.Count;
                result.Items = new List<T>();

                result.NextPossible = false;
            }
            else if(result.End >= list.Count)
            {
                // Start is in range of list but end is over
                result.End = list.Count;

                result.NextPossible = false;

                // No more items to show
                if(result.Start == result.End)
                {
                    result.Items = new List<T>();
                }
                else
                {
                    // Less than 'itemsPerPage' to show
                    result.Items = list.Skip(result.Start).Take(result.End - result.Start).ToList();
                }
            }
            else
            {
                // Both seem in range
                result.Items = list.Skip(result.Start).Take(result.End - result.Start).ToList();
            }

            if (result.Items.Count > 0)
            {
                if (result.Start > 0)
                {
                    result.PreviousPossible = true;
                }

                if (result.End < list.Count)
                {
                    result.NextPossible = true;
                }
            }

            return result;
        }
    }
}
