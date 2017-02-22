using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FrequentItemset
{
    public class PCY : Apriori
    {
        public PCY(List<string> candidates, List<string> transactions, int num_items, int num_transactions, decimal min_sup)
                    : base(candidates, transactions, num_items, num_transactions, min_sup)
        {
        }

        public override List<SupportElement> Execute(bool show_message = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            int[] count = new int[Candidates.Count];
            Dictionary<int, int> hashMap = new Dictionary<int, int>();
            for (int i = 0; i < NumTransactions; i++)
            {
                // aumento del count degli elementi delle transazioni
                string[] st_file = Transactions[i].Split(' ');
                foreach (string str in st_file)
                {
                    int value;
                    if (int.TryParse(str, out value))
                    {
                        count[value - 1]++;
                    }
                }

                // hash delle coppie e inserimento dentro h(x, y)
                List<string> candidates = GenerateCandidates(st_file.ToList(), 2);
                foreach (string candidate in candidates)
                {
                    string[] st = candidate.Split(' ');
                    int x, y;
                    if (int.TryParse(st[0], out x) && int.TryParse(st[1], out y))
                    {
                        int key = Hashing(x, y);
                        if (hashMap.ContainsKey(key))
                        {
                            hashMap[key]++;
                        }
                        else
                            hashMap.Add(key, 1);
                    }
                }
            }

            // Calcolo L1 e inserimento di L1 dentro l'output
            List<SupportElement> output = new List<SupportElement>();
            for (int i = 0; i < count.Length; i++)
            {
                if (count[i] >= MinSup)
                    output.Add(new SupportElement("" + (i + 1), count[i]));
            }

            if (show_message)
            {
                Console.WriteLine("C1 set => {0}", Candidates.Count);
                Console.WriteLine("L1 set => {0}", output.Count);
            }

            List<string> C = new List<string>();
            List<SupportElement> L;

            // Generiamo le coppie a partire da L1, in questo modo a, b E L1
            // se h(a, b) >= sup, allora inseriamo in C
            List<string> pairs = GenerateCandidates(output.Select(obj => obj.Label).ToList(), 2);
            foreach (string pair in pairs)
            {
                string[] st = pair.Split(' ');
                int x, y;
                if (int.TryParse(st[0], out x) && int.TryParse(st[1], out y))
                {
                    int key = Hashing(x, y);
                    if (hashMap.ContainsKey(key) && hashMap[key] >= MinSup)
                        C.Add(pair);
                }
            }

            if (show_message)
            {
                Console.WriteLine("C2 set => {0}", pairs.Count);
                Console.WriteLine("HashMap set => {0}", hashMap.Count);
                Console.WriteLine("C2 after Hash => {0}", C.Count);
            }

            L = CalculateFrequentItemsets(C);
            output.AddRange(L);
            if (show_message)
                Console.WriteLine("L2 => {0}", L.Count);

            int itemset = 2;
            do
            {
                itemset++;

                C = GenerateCandidates(L.Select(obj => obj.Label).ToList(), itemset);
                L = CalculateFrequentItemsets(C);
                if (show_message)
                {
                    Console.WriteLine("C{0} set => {1}", itemset, C.Count);
                    Console.WriteLine("L{0} set => {1}", itemset, L.Count);
                }

                output.AddRange(L);
            }
            while (L.Count > 1);

            stopwatch.Stop();
            if (show_message)
            {
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopwatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsed_time = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                //display output 
                Console.WriteLine("Output set => {0}", output.Count);

                //display the execution time
                Console.WriteLine("Execution time: {0}", elapsed_time);
            }

            return output;
        }

        private int Hashing(int x, int y)
        {
            return (x * 93199) + (y * 46559) % 50;
        }
    }
}
