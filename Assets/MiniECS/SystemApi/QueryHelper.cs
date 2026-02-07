using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace MiniECS
{
    internal static class QueryHelper<T>
    {
        public static readonly bool IsValueRW;
        public static readonly Type InnerType;
        public static readonly Func<Array, int, T> Create;

        static QueryHelper()
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(ValueRW<>))
            {
                IsValueRW = true;
                InnerType = typeof(T).GetGenericArguments()[0];
                var ctor = typeof(T).GetConstructor(new[] { typeof(Array), typeof(int) });
                if (ctor != null)
                {
                    var pArray = Expression.Parameter(typeof(Array), "arr");
                    var pIndex = Expression.Parameter(typeof(int), "idx");
                    var newExp = Expression.New(ctor, pArray, pIndex);
                    Create = Expression.Lambda<Func<Array, int, T>>(newExp, pArray, pIndex).Compile();
                }
            }
        }
    }
}
