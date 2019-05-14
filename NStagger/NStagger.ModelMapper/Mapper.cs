using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using java.lang;
using java.util;
using ArrayList = java.util.ArrayList;

namespace NStagger.ModelMapper
{
    public static class Mapper
    {
        public static T Map<T>(object stagger)
        {
            T output = (T)FormatterServices.GetSafeUninitializedObject(typeof(T));

            output.SetFieldValue(stagger, "trainingMode", "TrainingMode");

            output.SetFieldValue(stagger, "tokTypeTags", "TokenTypeTags");

            output.SetFieldValue(MapPerceptron(stagger, "posPerceptron"), "PosPerceptron");

            output.SetFieldValue(MapPerceptron(stagger, "nePerceptron"), "NePerceptron");

            output.SetFieldValue(stagger, "posBeamSize", "PosBeamSize");

            output.SetFieldValue(stagger, "neBeamSize", "NeBeamSize");

            output.SetFieldValue(stagger, "openTags", "OpenTags");

            output.SetFieldValue(stagger, "hasPos", "HasPos");

            output.SetFieldValue(MapHashSet<string>(stagger, "allowedPrefixes"), "AllowedPrefixes");

            output.SetFieldValue(MapHashSet<string>(stagger, "allowedSuffixes"), "AllowedSuffixes");

            output.SetProperty(MapTaggedData(stagger, "taggedData"), "TaggedData");

            output.SetPropertyFromField(stagger, "hasNE", "HasNe");

            output.SetProperty(MapLexicon(stagger, "posLexicon"), "PosLexicon");

            output.SetProperty(MapList<Dictionary>(stagger, "posDictionaries"), "PosDictionaries");

            output.SetProperty(MapList<Embedding>(stagger, "posEmbeddings"), "PosEmbeddings");

            output.SetProperty(MapList<Dictionary>(stagger, "neDictionaries"), "NeDictionaries");

            output.SetProperty(MapList<Embedding>(stagger, "neEmbeddings"), "NeEmbeddings");

            output.SetPropertyFromField(stagger, "extendLexicon", "ExtendLexicon");

            output.SetPropertyFromField(stagger, "maxPosIters", "MaximumPosIterations");

            output.SetPropertyFromField(stagger, "maxNEIters", "MaximumNeIterations");

            return output;
        }

        private static T MapTo<T>(this object obj, string fieldName, Dictionary<string, string> fields) where T : class
        {
            object value = obj.GetFieldValue(fieldName);

            if (value != null)
            {
                T output = (T)FormatterServices.GetSafeUninitializedObject(typeof(T));

                foreach (KeyValuePair<string, string> field in fields)
                {
                    output.SetFieldValue(value, field.Key, field.Value);
                }

                return output;
            }

            return null;
        }

        private static Perceptron MapPerceptron(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            Perceptron perceptron = obj.MapTo<Perceptron>(fieldName, new Dictionary<string, string> {

                { "sumWeight", "sumWeights"},

                {"oldSumWeight", "oldSumWeights"},

                {"curWeight", "currentWeights"},

                {"bestWeight", "bestWeights"},

                {"updateCount", "updateCounts"},

                {"bestUpdateCount", "bestUpdateCounts"}
            });

            if (value != null)
            {
                perceptron.SetFieldValue(MapHashSet(value, "featMap"), "featureMap");
            }

            return perceptron;
        }

        private static TaggedData MapTaggedData(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            if (value != null)
            {
                TaggedData output = (TaggedData)FormatterServices.GetSafeUninitializedObject(typeof(TaggedData));

                output.SetProperty(MapTagSet(value, "posTagSet"), "PosTagSet");

                output.SetProperty(MapTagSet(value, "neTagSet"), "NeTagSet");

                output.SetProperty(MapTagSet(value, "neTypeTagSet"), "NeTypeTagSet");

                output.SetPropertyFromField(value, "language", "Language");

                return output;
            }

            return null;
        }

        private static TagSet MapTagSet(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            if (value != null)
            {
                TagSet tagSet = (TagSet)FormatterServices.GetSafeUninitializedObject(typeof(TagSet));

                HashMap tagId = (HashMap)value.GetFieldValue("tagID");

                Dictionary<string, int> tagIds = tagId?.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => entry.Value is Integer integer ? integer.intValue() : (int)entry.Value);

                ArrayList tagName = (ArrayList)value.GetFieldValue("tagName");

                List<string> tagNames = tagName?.Cast<string>().ToList();

                tagSet.SetFieldValue(tagIds, "TagIds");

                tagSet.SetProperty(tagNames, "TagNames");

                return tagSet;
            }

            return null;
        }

        private static Lexicon MapLexicon(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            if (value != null)
            {
                Lexicon output = (Lexicon)FormatterServices.GetSafeUninitializedObject(typeof(Lexicon));

                HashMap lexicon = (HashMap)value.GetFieldValue("lexicon");

                Dictionary<string, List<Entry>> lexiconDictionary = lexicon.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => (from object o in (ArrayList)entry.Value select MapEntry(o)).ToList());

                output.SetFieldValue(lexiconDictionary, "lexicon");

                return output;
            }

            return null;
        }

        private static Entry MapEntry(object obj)
        {
            if (obj != null)
            {
                Entry output = (Entry)FormatterServices.GetSafeUninitializedObject(typeof(Entry));

                output.SetPropertyFromField(obj, "lf", "Lemma");

                output.SetPropertyFromField(obj, "tag", "TagId");

                output.SetPropertyFromField(obj, "n", "NumberOfOccurence");

                return output;
            }

            return null;
        }

        private static Dictionary MapDictionary(object obj)
        {
            if (obj != null)
            {
                HashMap hashMap = (HashMap)obj.GetFieldValue("map");

                Dictionary dictionary = (Dictionary)FormatterServices.GetSafeUninitializedObject(typeof(Dictionary));

                dictionary.SetProperty(hashMap.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value), "Map");

                return dictionary;
            }

            return null;
        }

        private static Embedding MapEmbedding(object obj)
        {
            if (obj != null)
            {
                HashMap hashMap = (HashMap)obj.GetFieldValue("map");

                Embedding embedding = (Embedding)FormatterServices.GetSafeUninitializedObject(typeof(Embedding));

                embedding.SetProperty(hashMap.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => entry.Value is Float[] floats ? floats.Select(f => f.floatValue()).ToArray() : (float[])entry.Value), "Map");

                return embedding;
            }

            return null;
        }

        private static List<T> MapList<T>(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            if (value != null)
            {
                if (typeof(T) == typeof(Dictionary))
                {
                    ArrayList list = (ArrayList)value;

                    return (from object o in list select MapDictionary(o)).Cast<T>().ToList();
                }
                else if (typeof(T) == typeof(Embedding))
                {
                    ArrayList list = (ArrayList)value;

                    return (from object o in list select MapEmbedding(o)).Cast<T>().ToList();
                }
            }

            return null;
        }

        private static HashSet<T> MapHashSet<T>(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            if (value != null)
            {
                HashSet hashSet = (HashSet)value;

                return new HashSet<T>((from object o in hashSet select o).Cast<T>());
            }

            return null;
        }

        private static Dictionary<string,int> MapHashSet(object obj, string fieldName)
        {
            object value = obj.GetFieldValue(fieldName);

            HashMap hashMap = (HashMap) value;

            return hashMap?.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => entry.Value is Integer floats ? floats.intValue() : (int)entry.Value);
        }
    }
}