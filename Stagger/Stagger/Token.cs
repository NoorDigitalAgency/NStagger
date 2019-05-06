using System.Linq;

namespace Stagger
{
    public class Token
    {
        public TokenType Type { get; }

        public string Value { get; }

        public int Offset { get; }

        public bool IsCapitalized { get; set; }

        public bool IsSpace { get; set; }

        public Token(TokenType type, string value, int offset)
        {
            Type = type;

            Value = value;

            Offset = offset;

            IsCapitalized = char.IsUpper(value[0]) && value.ToCharArray(1, value.Length - 1).Any(char.IsLower);

            IsSpace = type >= TokenType.Space && type <= TokenType.Newlines;
        }
    }
}
