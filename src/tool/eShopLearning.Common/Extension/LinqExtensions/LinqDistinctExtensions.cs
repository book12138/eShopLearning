using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eShopLearning.Common.Extension.LinqExtensions
{
    public static class LinqDistinctExtensions
    {
        /// <summary>
        /// 去重
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            if (source is null || source.Any() is false)
                return source;

            var hashSet = new HashSet<TKey>();

            var temp = source.ToList();
            for (int i = temp.Count - 1; i >= 0; i--)
            {
                var value = keySelector(temp[i]);
                if (hashSet.Contains(value) is false)
                {
                    hashSet.Add(value);
                    continue;
                }

                temp.RemoveAt(i);
            }

            return temp;
        }
    }
}
