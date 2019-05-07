using System.IO;

namespace Stagger
{
    public abstract class Tokenizer
    {
        public abstract Token Tokenize();

        public abstract void Close();

        public abstract Token[] ReadSentence();

        public abstract void Reset(StringReader reader);

        public abstract int CharactersCount { get; protected set; }

        public abstract string SentenceId { get; protected set; }
    }
}
