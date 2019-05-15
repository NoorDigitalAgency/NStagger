using System.Diagnostics;
using java.io;
using Console = System.Console;

namespace NStagger.ModelMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ObjectInputStream modelReader = new ObjectInputStream(new FileInputStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.bin"));

            se.su.ling.stagger.SUCTagger stagger = (se.su.ling.stagger.SUCTagger)modelReader.readObject();

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            stopwatch = Stopwatch.StartNew();

            Mapper.Map<SUCTagger>(stagger);

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            stopwatch = Stopwatch.StartNew();

            SUCTagger sucTagger = Mapper.Map<SUCTagger>();

            Console.WriteLine(stopwatch.Elapsed);

            Console.WriteLine(sucTagger.TaggedData.Language);

            stopwatch.Stop();
        }
    }
}
