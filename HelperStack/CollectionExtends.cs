using System.Collections;
using System.Collections.Generic;

namespace EBayAPI.Infrastructure
{
    public static class CollectionExtends
    {
        public static IList<T> ToList<T>(this CollectionBase coll)
        {
            var list = new List<T>();
            foreach (T item in coll)
            {
                list.Add(item);
            }

            return list;
        }
    }
}
