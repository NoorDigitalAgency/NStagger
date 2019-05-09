namespace Stagger
{
    public static class Character
    {
        public static int CharCount(int codePoint)
        {
            return codePoint >= 0x10000 ? 2 : 1;
        }

        public static bool IsHighSurrogate(char character)
        {
            return char.IsHighSurrogate(character);
        }

        public static int CodePointAt(char[] chars, int index, int limit)
        {
            string text = new string(chars, index, limit - index);

            return char.ConvertToUtf32(text, 0);
        }
    }
}
