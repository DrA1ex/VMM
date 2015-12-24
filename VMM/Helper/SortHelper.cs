using System.Collections.Generic;
using System.Linq;
using VMM.Model;

namespace VMM.Helper
{
    public static class SortHelper
    {
        public static IEnumerable<MusicEntry> Sort(IEnumerable<MusicEntry> enumerable, SortingPath[] paths)
        {
            if(paths == null || paths.Length == 0)
            {
                return enumerable;
            }

            var firstSortPath = paths.First();
            var sortable = firstSortPath.Descending
                ? enumerable.OrderByDescending(firstSortPath.Expression)
                : enumerable.OrderBy(firstSortPath.Expression);

            var additionalPaths = paths.Skip(1);
            foreach(var sortingPath in additionalPaths)
            {
                sortable = sortingPath.Descending
                    ? sortable.ThenByDescending(sortingPath.Expression)
                    : sortable.ThenBy(sortingPath.Expression);
            }

            return sortable.AsEnumerable();
        }
    }
}