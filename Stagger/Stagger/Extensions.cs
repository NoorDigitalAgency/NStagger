using System.Collections.Generic;

namespace Stagger
{
    public static class Extensions
    {
        public static bool IsEmpty<T>(this List<T> list)
        {
            return list.Count == 0;
        }
    }
}