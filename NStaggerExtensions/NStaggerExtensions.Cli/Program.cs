using System;
using System.Collections.Generic;
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

            foreach (string file in files)
            {
                string outputFile = Path.GetFileName(file);
                
                string text = File.ReadAllText(file);

                IEnumerable<string> enumerable = text.ToLines().ToList();
            
                text = enumerable.Aggregate((s, s1) => $"{s}{Environment.NewLine}{s1}");

                (string output, List<string> _) = swif.SegmentString(text);
            
                File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Desktop", "Outputs", outputFile), output);
            }
        }
    }
}
