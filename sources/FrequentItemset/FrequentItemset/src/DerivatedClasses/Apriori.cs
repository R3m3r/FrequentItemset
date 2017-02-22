using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FrequentItemset
{
    public class Apriori : BaseAlgorithm
    {
        public Apriori(List<string> candidates, List<string> transactions, int num_items, int num_transactions, decimal min_sup = 0)
                        : base(candidates, transactions, num_items, num_transactions, min_sup)
        {
        }

        public override List<SupportElement> Execute(bool show_message = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<string> C = Candidates;
            List<SupportElement> L = CalculateFrequentItemsets(C);

            if (show_message)
            {
                Console.WriteLine("C{0} set => {1}", 1, C.Count);
                Console.WriteLine("L{0} set => {1}", 1, L.Count);
            }

            int itemset = 1;
            List<SupportElement> output = new List<SupportElement>(L);
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

        protected virtual List<string> GenerateCandidates(List<string> candidates, int n)
        {
            List<string> temp_candidates = new List<string>();
            string str1, str2;
            string[] st1, st2;
            StringBuilder str_builder = new StringBuilder();

            if (n == 2)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    str1 = candidates[i];
                    for (int j = i + 1; j < candidates.Count; j++)
                    {
                        str2 = candidates[j];
                        temp_candidates.Add(str1 + " " + str2);
                    }
                }
            }
            else if (n > 2)
            {
                for (int i = 0; i < candidates.Count; i++)
                {
                    for (int j = i + 1; j < candidates.Count; j++)
                    {
                        str_builder.Clear();
                        str1 = "";
                        str2 = "";
                        st1 = candidates[i].Split(' ');
                        st2 = candidates[j].Split(' ');

                        int k = 0;
                        for (int s = 0; s < n - 2; s++)
                        {
                            if (k < st1.Length)
                                str1 += " " + st1[k];
                            if (k < st2.Length)
                                str2 += " " + st2[k];
                            k++;
                        }

                        if (str2.Equals(str1, StringComparison.OrdinalIgnoreCase) && k < st1.Length && k < st2.Length)
                        {
                            temp_candidates.Add(str_builder.Append(str1).Append(" ").Append(st1[k]).Append(" ").Append(st2[k]).ToString().Trim());
                        }
                    }
                }
            }
            return temp_candidates;
        }

        protected virtual int[] GetSupport(List<string> candidates)
        {
            bool match = false;
            bool[] trans = new bool[NumItems];
            int[] count = new int[candidates.Count];

            for (int i = 0; i < NumTransactions; i++)
            {
                for (int j = 0; j < NumItems; j++)
                {
                    trans[j] = false;
                }

                string[] st_file = Transactions[i].Split(' ');
                foreach (string element in st_file)
                {
                    int num;
                    if (element != null && int.TryParse(element, out num))
                    {
                        int index = num - 1;
                        trans[index] = true;
                    }
                }

                for (int c = 0; c < candidates.Count; c++)
                {
                    match = false;
                    string[] st = candidates[c].Split(' ');
                    foreach (string element in st)
                    {
                        int num;
                        if (element != null && int.TryParse(element, out num))
                        {
                            match = trans[num - 1];
                            if (!match)
                                break;
                        }
                    }
                    if (match)
                        count[c]++;
                }
            }

            return count;
        }

        protected virtual List<SupportElement> CalculateFrequentItemsets(List<string> candidates)
        {
            List<SupportElement> frequent_candidates = new List<SupportElement>();
            int[] count = GetSupport(candidates);
            for (int i = 0; i < candidates.Count; i++)
            {
                if ((count[i]) >= MinSup)
                {
                    frequent_candidates.Add(new SupportElement(candidates[i], count[i]));
                }
            }
            return frequent_candidates;
        }
    }
}
