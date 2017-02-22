using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FrequentItemset
{
    public class Sampling : Apriori
    {
        public const double FIXED_PROBABILITY = 0.1d;

        private List<string> global_transactions;
        private decimal global_min_sup;

        public Sampling(List<string> candidates, List<string> transactions, int num_items, int num_transactions, decimal min_sup = 0, double fix_prob = 0)
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

            global_transactions = transactions;
            global_min_sup = min_sup;
        }

        public override List<SupportElement> Execute(bool show_message = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            List<SupportElement> output = base.Execute(show_message);

            m_candidates = output.Select(obj => obj.Label).ToList();
            m_transactions = global_transactions;
            m_num_transactions = global_transactions.Count;
            m_min_sup = (int)(global_min_sup * m_num_transactions);

            if (show_message)
            {
                Console.WriteLine("Filtering false positive");
            }

            output = CalculateFrequentItemsets(m_candidates);

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
    }
}
