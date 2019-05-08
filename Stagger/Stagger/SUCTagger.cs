using System.Collections.Generic;
using System.Diagnostics;

namespace Stagger
{
    public class SUCTagger : Tagger
    {
        public SUCTagger(TaggedData taggedData, int posBeamSize, int neBeamSize) : base(taggedData, posBeamSize, neBeamSize)
        {
        }

        private string CapitalizeLemma(string lowerCaseText, int posTag)
        {
            try
            {
                if (posTag == TaggedData.PosTagSet.GetTagId("PM|NOM"))
                {
                    return $"{lowerCaseText.Substring(0, 1).ToUpper()}{lowerCaseText.Substring(1)}";
                }
                else if (posTag == TaggedData.PosTagSet.GetTagId("PM|GEN"))
                {
                    int suffixLen = 0;

                    if (lowerCaseText.EndsWith(":s"))
                    {
                        suffixLen = 2;
                    }
                    else if (lowerCaseText.EndsWith("s"))
                    {
                        suffixLen = 1;
                    }

                    return $"{lowerCaseText.Substring(0, 1).ToUpper()}{lowerCaseText.Substring(1, lowerCaseText.Length - suffixLen)}";
                }
            }
            catch (TagNameException)
            {
            }

            return lowerCaseText;
        }

        protected override string GetLemma(TaggedToken token)
        {
            int posTag = token.PosTag;

            string lowerCaseText = token.LowerCaseText;

            try
            {
                if (posTag == TaggedData.PosTagSet.GetTagId("LE"))
                {
                    return token.Token.Value;
                }
            }
            catch (TagNameException)
            {
            }

            Entry[] entries = PosLexicon.GetEntries(lowerCaseText);

            if (entries != null)
            {
                foreach (Entry entry in entries)
                {
                    if (entry.TagId == posTag && entry.Lemma != null)
                    {
                        return entry.Lemma;
                    }
                }
            }

            int length = lowerCaseText.Length;

            for (int i = (length <= 16) ? 1 : length - 16; i < length; i++)
            {
                entries = PosLexicon.GetEntries(lowerCaseText.Substring(i));

                if (entries == null) continue;

                foreach (Entry entry in entries)
                {
                    if (entry.TagId == posTag && entry.Lemma != null)
                    {
                        return CapitalizeLemma(lowerCaseText.Substring(0, i) + entry.Lemma.ToLower(), posTag);
                    }
                }
            }

            return CapitalizeLemma(lowerCaseText, posTag);
        }

        public override void BuildLexicons(TaggedToken[][] sentences)
        {
            base.BuildLexicons(sentences);

            try
            {
                TokenTypeTags[(int)TokenType.Smiley] = new int[1];

                TokenTypeTags[(int)TokenType.Smiley][0] = TaggedData.PosTagSet.GetTagId("LE", true);

                PosLexicon.Interpolate(TaggedData.PosTagSet.GetTagId("NN|NEU|PLU|IND|NOM"), TaggedData.PosTagSet.GetTagId("NN|NEU|SIN|IND|NOM"));
            }
            catch (TagNameException)
            {
                Debug.Assert(false);
            }
        }

        protected override void ComputeOpenTags()
        {
            string[] names = TaggedData.PosTagSet.GetTagNames;

            int[] tags = new int[names.Length];

            string[] openTagArray = { "NN", "VB", "JJ", "AB", "PC", "RG", "RO", "PM", "UO", "LE" };

            HashSet<string> openTagSet = new HashSet<string>(openTagArray);

            int nTags = 0;

            for (int i = 0; i < names.Length; i++)
            {
                if (openTagSet.Contains(names[i].Split('|')[0]))
                {
                    tags[nTags++] = i;
                }
            }

            Debug.Assert(nTags > 0);

            OpenTags = Arrays.CopyOf(tags, nTags);
        }
    }
}
