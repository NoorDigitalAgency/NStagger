namespace Stagger
{
    public class TaggedToken
    {
        public Token Token { get; }

        public string Id { get; }

        public string LowerCaseText { get; }

        public string Lemma { set; get; }

        public int PosTag { set; get; }

        public int NeTag { set; get; }

        public int NeTypeTag { set; get; }

        public TaggedToken(Token token, string id)
        {
            Token = token;

            Id = id;

            LowerCaseText = token.Value.ToLower();

            Lemma = null;

            PosTag = -1;

            NeTag = -1;

            NeTypeTag = -1;
        }

        public TaggedToken(TaggedToken taggedToken)
        {
            Token = taggedToken.Token;

            LowerCaseText = taggedToken.LowerCaseText;

            Id = taggedToken.Id;

            Lemma = taggedToken.Lemma;

            PosTag = taggedToken.PosTag;

            NeTag = taggedToken.NeTag;

            NeTypeTag = taggedToken.NeTypeTag;
        }

        public bool ConsistentWith(TaggedToken taggedToken)
        {
            if (PosTag >= 0 && taggedToken.PosTag >= 0 && PosTag != taggedToken.PosTag)
            {
                return false;
            }

            if (Lemma != null && taggedToken.Lemma != null && !Lemma.Equals(taggedToken.Lemma))
            {
                return false;
            }

            if (NeTag >= 0 && taggedToken.NeTag >= 0 && NeTag != taggedToken.NeTag)
            {
                return false;
            }

            return NeTypeTag < 0 || taggedToken.NeTypeTag < 0 || NeTypeTag == taggedToken.NeTypeTag;
        }
    }
}