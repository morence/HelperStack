using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperStack
{
    public static class PagingStructure
    {
        /// <summary>
        /// paging deals with big data
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="pageSize"></param>
        /// <param name="singlePageProcess"></param>
        public static void ToPagingProcess<T>(this IEnumerable<T> sourceData, int pageSize, Action<IEnumerable<T>> singlePageProcess) where T : class
        {
            if (sourceData != null && sourceData.Count() > 0)
            {
                var cnt = sourceData.Count();
                var totalPages = sourceData.Count() / pageSize;
                if (cnt % pageSize > 0) totalPages += 1;

                for (int pageIndex = 1; pageIndex <= totalPages; pageIndex++)
                {
                    var currentPageItems = sourceData.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    singlePageProcess(currentPageItems);
                }
            }
        }
    }
}
