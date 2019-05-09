using System.IO;

namespace Stagger
{
    public partial class LatinTokenizer : Tokenizer
    {
        public override Token Tokenize()
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            throw new System.NotImplementedException();
        }

        public override Token[] ReadSentence()
        {
            throw new System.NotImplementedException();
        }

        public override void Reset(StringReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override int CharactersCount { get; protected set; }

        public override string SentenceId { get; protected set; }
    }
}