using System;
using System.Collections.Generic;
using System.Linq;

namespace PharmaMoov.API.Helpers
{
    public static class Extensions
    {
        public static IEnumerable<TEntity> Where<TEntity>(this IEnumerable<TEntity> value, Func<TEntity, bool> predicate, int _pageNumber, int _pageSize) where TEntity : class
        {
            var skipCount = _pageNumber * _pageSize;
            return value.Where(predicate).Skip(skipCount).Take(_pageSize);
        }
    }
}
