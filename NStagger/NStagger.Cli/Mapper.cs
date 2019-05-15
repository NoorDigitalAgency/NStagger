using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NStagger.Model;

namespace NStagger.Cli
{
    public static class Mapper
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();

        private static FileStream fileStream;

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

        private static Container Read()
        {
            return (Container)formatter.Deserialize(fileStream);
        }
    }
}