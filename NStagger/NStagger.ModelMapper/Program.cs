using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using java.io;

namespace NStagger.ModelMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            ObjectInputStream modelReader = new ObjectInputStream(new FileInputStream(@"C:\Users\Rojan\Desktop\swedish.bin\t.bin"));

            se.su.ling.stagger.SUCTagger stagger = (se.su.ling.stagger.SUCTagger)modelReader.readObject();

            SUCTagger sucTagger = Mapper.Map<SUCTagger>(stagger);

            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(new FileStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nbin", FileMode.Create), sucTagger);
        }
    }
}
