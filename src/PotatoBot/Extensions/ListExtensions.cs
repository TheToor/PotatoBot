using PotatoBot.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PotatoBot.Extensions
{
    public static class ListExtensions
    {
        public static PageResult<T> TakePaged<T>(this IEnumerable<T> list, int page, int itemsPerPage)
        {
            var itemCount = list.Count();
            if(itemCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(list));
            }
            if(page < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(page), page, "Page needs to be greaten than 0");
            }

            var result = new PageResult<T>
            {
                Start = page * itemsPerPage
            };
            result.End = result.Start + itemsPerPage;

            if(result.Start >= itemCount)
            {
                // We are over the end so just return an empty List

                if(result.Start - itemsPerPage < itemCount)
                {
                    result.PreviousPossible = true;
                }

                result.Start = itemCount;
                result.End = itemCount;
                result.Items = new List<T>();

                result.NextPossible = false;
            }
            else if(result.End >= itemCount)
            {
                // Start is in range of list but end is over
                result.End = itemCount;

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

            if(result.Items.Count > 0)
            {
                if(result.Start > 0)
                {
                    result.PreviousPossible = true;
                }

                if(result.End < itemCount)
                {
                    result.NextPossible = true;
                }
            }

            return result;
        }
    }
}
