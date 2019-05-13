namespace NStagger
{
    public class History
    {
        public string Text { get; }

        public string LowerCaseText { get; }

        public string Lemma { get; }

        public int PosTag { get; }

        public int NeTag { get; }

        public int NeTypeTag { get; }

        public double Score { get; }

        public History Last { get; }

        public History(string text, string lowerCaseText, string lemma, int posTag, int neTag, int neTypeTag, double score, History last)
        {
            Text = text;

            LowerCaseText = lowerCaseText;

            Lemma = lemma;

            PosTag = posTag;

            NeTag = neTag;

            NeTypeTag = neTypeTag;

            Score = score;

            Last = last;
        }
    }
}