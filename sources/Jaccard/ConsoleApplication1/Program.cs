using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> Apriori = new List<string>();
            List<string> Sampling = new List<string>();

            string line;
            // leggo l'insieme apriori
            try
            {
                StreamReader data_in = new StreamReader(args[0]);
                while ((line = data_in.ReadLine()) != null)
                    Apriori.Add(line);
                data_in.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }

            // leggo l'insieme sampling
            try
            {
                StreamReader data_in = new StreamReader(args[1]);
                while ((line = data_in.ReadLine()) != null)
                    Sampling.Add(line);
                data_in.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }

			// J(A, B) = |A and B| / |A or B|
            List<string> Union = Apriori.Union(Sampling).ToList();
            List<string> Intersect = Apriori.Intersect(Sampling).ToList();
            double sim = (double)Intersect.Count / (double)Union.Count;

            Console.WriteLine("Jaccard {0}", sim);
        }
    }
}
