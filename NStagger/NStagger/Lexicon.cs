using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NStagger
{
    [Serializable]
    public class Lexicon
    {
        private Dictionary<string, List<Entry>> lexicon;

        public Lexicon()
        {
            Clear();
        }

        public void Clear()
        {
            lexicon = new Dictionary<string, List<Entry>>();
        }

        public int Size => lexicon.Count;

        public void AddEntry(string wordForm, string lemma, int tagId, int numberOfOccurence)
        {
            string wordFormLower = wordForm.ToLower();

            List<Entry> entries;

            if (!lexicon.ContainsKey(wordFormLower))
            {
                entries = new List<Entry>(4);

                lexicon[wordFormLower] = entries;
            }
            else
            {
                entries = lexicon[wordFormLower];
            }

            AddEntry(entries, lemma, tagId, numberOfOccurence);
        }

        private static void AddEntry(IList<Entry> entries, string lemma, int tagId, int numberOfOccurence)
        {
            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];

                if (entry.TagId > tagId)
                {
                    entries.Insert(i, new Entry(lemma, tagId, numberOfOccurence));

                    return;
                }
                else if (tagId == entry.TagId)
                {
                    string newLemma = lemma ?? entry.Lemma;

                    entries[i] = new Entry(newLemma, tagId, numberOfOccurence + entry.NumberOfOccurence);

                    return;
                }
            }

            entries.Add(new Entry(lemma, tagId, numberOfOccurence));
        }

        public Entry[] GetEntries(string wordForm)
        {
            if (!lexicon.ContainsKey(wordForm.ToLower()))
            {
                return null;
            }

            List<Entry> entries = lexicon[wordForm.ToLower()];

            return entries?.ToArray();
        }

        public int CountWordForm(string wordForm)
        {
            if (!lexicon.ContainsKey(wordForm.ToLower())) return 0;

            int numberOfOccurence = 0;

            foreach (Entry entry in lexicon[wordForm.ToLower()])
            {
                numberOfOccurence += entry.NumberOfOccurence;
            }

            return numberOfOccurence;
        }

        public bool IsInLexicon(string wordForm)
        {
            return lexicon.ContainsKey(wordForm.ToLower());
        }

        public void Interpolate(int tagId1, int tagId2)
        {
            HashSet<string> suffixSet = new HashSet<string>();

            foreach (KeyValuePair<string, List<Entry>> pair in lexicon)
            {
                string wordForm = pair.Key;

                List<Entry> entries = pair.Value;

                Entry entry1 = null, entry2 = null;

                foreach (Entry entry in entries)
                {
                    if (entry.TagId == tagId1)
                    {
                        entry1 = entry;
                    }
                    else if (entry.TagId == tagId2)
                    {
                        entry2 = entry;
                    }
                }

                if (entry1 != null && entry2 != null)
                {
                    suffixSet.Add(wordForm);
                }
            }

            foreach (KeyValuePair<string, List<Entry>> pair in lexicon)
            {
                string wordForm = pair.Key;

                List<Entry> entries = pair.Value;

                Entry entry1 = null, entry2 = null;

                foreach (Entry entry in entries)
                {
                    if (entry.TagId == tagId1)
                    {
                        entry1 = entry;
                    }
                    else if (entry.TagId == tagId2)
                    {
                        entry2 = entry;
                    }
                }

                if (entry1 == null && entry2 == null) continue;

                if (entry1 != null && entry2 != null) continue;

                bool hasSuffix = false;

                for (int i = 2; i < wordForm.Length; i++)
                {
                    if (suffixSet.Contains(wordForm.Substring(i)))
                    {
                        hasSuffix = true;

                        break;
                    }
                }

                if (!hasSuffix) continue;

                if (entry1 != null)
                {
                    AddEntry(entries, entry1.Lemma, tagId2, 0);
                }
                else
                {
                    AddEntry(entries, entry2.Lemma, tagId1, 0);
                }
            }
        }

        public void FromStreamReader(StreamReader reader, TagSet tagSet, bool extend)
        {
            string line;

            HashSet<string> known = null;

            if (!extend) known = new HashSet<string>(lexicon.Keys);

            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split('\t');

                if (fields.Length >= 4)
                {
                    string wf = fields[0];

                    if (!extend && known.Contains(wf.ToLower())) continue;

                    string lf = fields[1];

                    int tag = tagSet.GetTagId(fields[2], true);

                    int n = int.Parse(fields[3]);

                    AddEntry(wf, lf, tag, n);
                }
            }
        }

        public void FromFile(string filePath, TagSet tagSet, bool extend)
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                FromStreamReader(reader, tagSet, extend);

                reader.Close();
            }
        }
    }
}