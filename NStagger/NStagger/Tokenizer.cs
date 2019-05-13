using System.Collections.Generic;
using System.IO;

namespace NStagger
{
    public abstract class Tokenizer
    {
        public abstract Token Tokenize();

        public abstract void Close();

        public abstract List<Token> ReadSentence();

        public abstract void Reset(TextReader reader);

        public abstract int CharactersCount { get; protected set; }

        public abstract string SentenceId { get; protected set; }
    }
}
