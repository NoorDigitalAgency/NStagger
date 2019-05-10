using System;

namespace Stagger
{
    [Serializable]
    public class Entry
    {
        public Entry(string lemma, int tagId, int numberOfOccurence)
        {
            Lemma = lemma;

            TagId = tagId;

            NumberOfOccurence = numberOfOccurence;
        }

        public string Lemma { get; }

        public int TagId { get; }

        public int NumberOfOccurence { get; }
    }
}