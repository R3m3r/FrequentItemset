using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FrequentItemset
{
    public class Toivonen : Apriori
    {
        public const double FIXED_PROBABILITY = 0.1d;

        private List<SupportElement> m_negative_border;

        private List<string> global_transactions;
        private decimal global_min_sup;

        public Toivonen(List<string> candidates, List<string> transactions, int num_items, int num_transactions, decimal min_sup = 0, double fix_prob = 0)
                        : base(candidates, transactions, num_items, num_transactions, min_sup)
        {
            double fixed_prob = fix_prob;
            if (fixed_prob == 0)
                fixed_prob = FIXED_PROBABILITY;

            m_transactions = new List<string>();
            Random rnd = new Random(DateTime.Now.Millisecond);
            for (int i = 0; i < num_transactions; i++)
            {
                if (rnd.NextDouble() < fixed_prob)
                    Transactions.Add((string)transactions[i].Clone());
            }

            m_min_sup = (int)(min_sup * num_transactions * (decimal)FIXED_PROBABILITY);
            m_num_transactions = Transactions.Count;
            m_negative_border = new List<SupportElement>();

            global_transactions = transactions;
            global_min_sup = min_sup;
        }

        public override List<SupportElement> Execute(bool show_message = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<SupportElement> output = base.Execute(show_message);
            List<SupportElement> negative_border;

            foreach (SupportElement element in m_negative_border)
            {
                var list = element.Label.Split(' ').ToList();
                var result = GetPermutations(list, list.Count - 1);

                // ottengo le permutazioni
                List<string> permutations = new List<string>();
                foreach (var perm in result)
                {
                    string tmp = "";
                    foreach (var c in perm)
                    {
                        tmp += c + " ";
                    }
                    tmp = tmp.Trim();
                    if (!string.IsNullOrEmpty(tmp))
                        permutations.Add(tmp);
                }

                // verifico che ogni sottoinsieme sia frequente
                bool match = true;
                foreach (string permutation in permutations)
                {
                    if (!output.Any(x => x.Label.Equals(permutation)))
                    {
                        match = false;
                        break;
                    }
                }

                if (!match)
                    element.Count = -1;
            }

            negative_border = m_negative_border.Where(x => x.Count >= 0).ToList();
            if (show_message)
            {
                Console.WriteLine("Negative Border set => {0}", negative_border.Count);
                Console.WriteLine("Filtering false positive");
            }

            m_transactions = global_transactions;
            m_num_transactions = global_transactions.Count;
            m_min_sup = (int)(global_min_sup * m_num_transactions);

            // Ricerca degli insiemi frequenti nella frontiera negativa e nel campione
            m_candidates = output.Select(obj => obj.Label).ToList();
            output = CalculateFrequentItemsets(m_candidates);
            m_candidates = negative_border.Select(obj => obj.Label).ToList();
            negative_border = CalculateFrequentItemsets(m_candidates);

            if (show_message)
            {
                if (negative_border.Count == 0)
                {
                    Console.WriteLine("No member of the negative border is frequent in the whole dataset.");
                }
                else
                {
                    Console.WriteLine("{0} member of the negative border is frequent in the whole. Repeat the algorithm with new sample.", negative_border.Count);
                }
            }

            stopwatch.Stop();
            if (show_message)
            {
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopwatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsed_time = string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

                //display output 
                Console.WriteLine("Final Output set => {0}", output.Count);

                //display the execution time
                Console.WriteLine("Final Execution time: {0}", elapsed_time);
            }

            return output;
        }

        protected override List<SupportElement> CalculateFrequentItemsets(List<string> candidates)
        {
            List<SupportElement> frequent_candidates = new List<SupportElement>();
            int[] count = GetSupport(candidates);
            for (int i = 0; i < candidates.Count; i++)
            {
                SupportElement element = new SupportElement(candidates[i], count[i]);
                if ((count[i]) >= MinSup)
                {
                    frequent_candidates.Add(element);
                }
                else
                {
                    m_negative_border.Add(element);
                }
            }

            return frequent_candidates;
        }

        private IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> items, int count)
        {
            if (count == 0)
                yield return new T[] {};

            int i = 0;
            foreach (var item in items)
            {
                if (count == 1)
                    yield return new T[] { item };
                else
                {
                    foreach (var result in GetPermutations(items.Skip(i + 1), count - 1))
                        yield return new T[] { item }.Concat(result);
                }

                ++i;
            }
        }
    }
}

