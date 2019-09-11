using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Swif.Core;
using Swif.Extensions;

namespace NStaggerExtensions.Cli
{
    class Program
    {
        static void Main()
        {
            string text = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "Words Frequency Lists", "Output", "Test.txt"));
            
            Stopwatch stopwatch1 = Stopwatch.StartNew();

            text = text.ToLines().Aggregate((s, s1) => $"{s}{Environment.NewLine}{s1}");

            SwedishWordIdentifierByFrequency swif = new SwedishWordIdentifierByFrequency(5);
            
            stopwatch1.Stop();
            
            Console.WriteLine($"{stopwatch1.Elapsed:g}");

            Stopwatch stopwatch2 = Stopwatch.StartNew();

            (string output, List<string> list) = swif.SegmentString(text);

            stopwatch2.Stop();
            
            Console.WriteLine($"{stopwatch2.Elapsed:g}");
            
            File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "Words Frequency Lists", "Output", "Compare.txt"), output);
        }
    }
}
