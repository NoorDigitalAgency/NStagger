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
            SwedishWordIdentifierByFrequency swif = new SwedishWordIdentifierByFrequency(5);

            string[] files = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "Inputs"), "*.txt");

            Stopwatch stopwatch = new Stopwatch();

            List<long> times = new List<long>();
            
            foreach (string file in files)
            {
                stopwatch.Restart();
                
                string outputFile = Path.GetFileName(file);

                string text = File.ReadAllText(file);

                IEnumerable<string> enumerable = text.ToLines(pointDoubleLineBreak: false).ToList();

                text = enumerable.Aggregate((s, s1) => $"{s}{Environment.NewLine}{s1}");

                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "Outputs", outputFile), swif.SegmentString(text).output);

                stopwatch.Stop();
                
                times.Add(stopwatch.ElapsedMilliseconds);
            }
            
            Console.WriteLine($"Count: {times.Count} - Average: {times.Average()} - Maximum: {times.Max()},  Minimum: {times.Min()}");

            Console.ReadLine();
        }
    }
}