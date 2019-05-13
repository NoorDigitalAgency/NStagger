using System;

namespace NStagger
{
    [Serializable]
    public class PTBTagger : Tagger
    {
        public PTBTagger(TaggedData taggedData, int posBeamSize, int neBeamSize) : base(taggedData, posBeamSize, neBeamSize)
        {
        }

        protected override string GetLemma(TaggedToken token)
        {
            return null;
        }

        private readonly string[] openTagArray = { "CD", "FW", "IN", "JJ", "JJR", "JJS", "NN", "NNS", "NNP", "NNPS", "RB", "RBR", "RBS", "SYM", "UH", "VB", "VBD", "VBG", "VBN", "VBP", "VBZ" };

        protected override void ComputeOpenTags()
        {
            OpenTags = new int[openTagArray.Length];

            TagSet tagSet = TaggedData.PosTagSet;

            for (int i = 0; i < OpenTags.Length; i++)
            {
                try
                {
                    OpenTags[i] = tagSet.GetTagId(openTagArray[i]);
                }
                catch (TagNameException)
                {
                    Console.WriteLine($"Open tag not in tag set: {openTagArray[i]}");

                    Environment.Exit(1);
                }
            }
        }
    }
}