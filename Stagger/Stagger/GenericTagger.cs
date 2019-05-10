using System;

namespace Stagger
{
    [Serializable]
    public class GenericTagger : Tagger
    {
        public GenericTagger(TaggedData taggedData, int posBeamSize, int neBeamSize) : base(taggedData, posBeamSize, neBeamSize)
        {
        }

        public override void Train(TaggedToken[][] trainSentence, TaggedToken[][] developmentSentence)
        {
            Console.WriteLine($"POS lexicon size before '{PosLexicon.Size}'.");

            BuildLexicons(trainSentence);

            Console.WriteLine($"POS lexicon size after '{PosLexicon.Size}'.");

            base.Train(trainSentence, developmentSentence);
        }

        protected override string GetLemma(TaggedToken token)
        {
            return null;
        }

        protected override void ComputeOpenTags()
        {
            int tagsCount = TaggedData.PosTagSet.Size;

            OpenTags = new int[tagsCount];

            for (int i = 0; i < tagsCount; i++)
            {
                OpenTags[i] = i;
            }
        }
    }
}