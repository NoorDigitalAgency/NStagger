using System.Collections.Generic;

namespace NStagger
{
    public static class Extensions
    {
        public static bool IsEmpty<T>(this List<T> list)
        {
            return list.Count == 0;
        }
    }
}