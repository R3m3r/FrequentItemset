using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Duplicator
{
    class Program
    {
        static void Main(string[] args)
        {
            var originalLines = File.ReadAllLines(args[0]);
            var shuffledLines = originalLines.OrderBy(line => Guid.NewGuid()).ToArray();
            File.WriteAllLines("test.txt", shuffledLines);
        }
    }
}
