using System;
using System.Collections.Generic;
using System.Linq;

namespace NStagger.Cli
{
    public static class Collections
    {
        public static void Sort<T>(List<T> list)
        {
            T[] array = list.OrderBy(arg => arg).ToArray();

            for (int i = 0; i < array.Length; i++)
            {
                list[i] = array[i];
            }
        }

        public static void Shuffle<T>(List<T> list, Random random)
        {
            int n = list.Count;

            while (n > 1)
            {
                n--;

                int k = random.Next(n + 1);

                T value = list[k];

                list[k] = list[n];

                list[n] = value;
            }
        }
    }
}