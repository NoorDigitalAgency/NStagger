using System;

namespace NStagger
{
    public static class Arrays
    {
        public static T[] CopyOf<T>(T[] array, int newLength)
        {
            if (array.Length == newLength)
            {
                return new Span<T>(array).ToArray();
            }
            else
            {
                Span<T> span = new T[newLength];

                (newLength < array.Length ? new Span<T>(array).Slice(0, newLength) : new Span<T>(array)).CopyTo(span);

                return span.ToArray();
            }
        }
    }
}
