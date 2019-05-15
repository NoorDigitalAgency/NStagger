using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using java.lang;
using java.util;
using ArrayList = java.util.ArrayList;

namespace NStagger.ModelMapper
{
    public static class Mapper
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();

        private static FileStream fileStream;

        public static void Map<T>(object stagger)
        {
            fileStream = new FileStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nbin", FileMode.Create);

            T output = (T)FormatterServices.GetSafeUninitializedObject(typeof(T));

            output.SetFieldValue(Write(stagger.GetFieldValue("trainingMode"), "trainingMode"), "TrainingMode");

            output.SetFieldValue(Write(stagger.GetFieldValue("tokTypeTags"), "tokTypeTags"), "TokenTypeTags");

            output.SetFieldValue(Write(MapPerceptron(stagger, "posPerceptron"), "posPerceptron"), "PosPerceptron");

            output.SetFieldValue(Write(MapPerceptron(stagger, "nePerceptron"), "nePerceptron"), "NePerceptron");

            output.SetFieldValue(Write(stagger.GetFieldValue("posBeamSize"), "posBeamSize"), "PosBeamSize");

            output.SetFieldValue(Write(stagger.GetFieldValue("neBeamSize"), "neBeamSize"), "NeBeamSize");

            output.SetFieldValue(Write(stagger.GetFieldValue("openTags"), "openTags"), "OpenTags");

            output.SetFieldValue(Write(stagger.GetFieldValue("hasPos"), "hasPos"), "HasPos");

            output.SetFieldValue(Write(MapHashSet<string>(stagger, "allowedPrefixes"), "allowedPrefixes"), "AllowedPrefixes");

            output.SetFieldValue(Write(MapHashSet<string>(stagger, "allowedSuffixes"), "allowedSuffixes"), "AllowedSuffixes");

            output.SetProperty(Write(MapTaggedData(stagger, "taggedData"), "taggedData"), "TaggedData");

            output.SetProperty(Write(stagger.GetFieldValue("hasNE"), "hasNE"), "HasNe");

            output.SetProperty(Write(MapLexicon(stagger, "posLexicon"), "posLexicon"), "PosLexicon");

            output.SetProperty(Write(MapList<Dictionary>(stagger, "posDictionaries"), "posDictionaries"), "PosDictionaries");

            output.SetProperty(Write(MapList<Embedding>(stagger, "posEmbeddings"), "posEmbeddings"), "PosEmbeddings");

            output.SetProperty(Write(MapList<Dictionary>(stagger, "neDictionaries"), "neDictionaries"), "NeDictionaries");

            output.SetProperty(Write(MapList<Embedding>(stagger, "neEmbeddings"), "neEmbeddings"), "NeEmbeddings");

            output.SetProperty(Write(stagger.GetFieldValue("extendLexicon"), "extendLexicon"), "ExtendLexicon");

            output.SetProperty(Write(stagger.GetFieldValue("maxPosIters"), "maxPosIters"), "MaximumPosIterations");

            output.SetProperty(Write(stagger.GetFieldValue("maxNEIters"), "maxNEIters"), "MaximumNeIterations");

            fileStream.Flush();

            fileStream.Close();

            fileStream.Dispose();
        }

        public static T Map<T>()
        {
            fileStream = new FileStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nbin", FileMode.Open);

            T output = (T)FormatterServices.GetSafeUninitializedObject(typeof(T));

            output.SetFieldValue(Read().Obj, "TrainingMode");

            output.SetFieldValue(Read().Obj, "TokenTypeTags");

            output.SetFieldValue(Read().Obj, "PosPerceptron");

            output.SetFieldValue(Read().Obj, "NePerceptron");

            output.SetFieldValue(Read().Obj, "PosBeamSize");

            output.SetFieldValue(Read().Obj, "NeBeamSize");

            output.SetFieldValue(Read().Obj, "OpenTags");

            output.SetFieldValue(Read().Obj, "HasPos");

            output.SetFieldValue(Read().Obj, "AllowedPrefixes");

            output.SetFieldValue(Read().Obj, "AllowedSuffixes");

            output.SetProperty(Read().Obj, "TaggedData");

            output.SetProperty(Read().Obj, "HasNe");

            output.SetProperty(Read().Obj, "PosLexicon");

            output.SetProperty(Read().Obj, "PosDictionaries");

            output.SetProperty(Read().Obj, "PosEmbeddings");

            output.SetProperty(Read().Obj, "NeDictionaries");

            output.SetProperty(Read().Obj, "NeEmbeddings");

            output.SetProperty(Read().Obj, "ExtendLexicon");

            output.SetProperty(Read().Obj, "MaximumPosIterations");

            output.SetProperty(Read().Obj, "MaximumNeIterations");

            fileStream.Close();

            fileStream.Dispose();

            return output;
        }

        private static object Write(object value, string name)
        {
            Container container = new Container { Obj = value };

            try
            {
                formatter.Serialize(fileStream, container);

                fileStream.Flush();
            }
            catch
            {
                Console.WriteLine($"Failed on '{name}'.");

                throw;
            }

            return value;
        }

        private static Container Read()
        {
            return (Container)formatter.Deserialize(fileStream);
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