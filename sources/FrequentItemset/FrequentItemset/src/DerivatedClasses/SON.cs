using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Text;

namespace FrequentItemset
{
    public class SON : Apriori
    {
        private class ConcurrentApriori : Apriori
        {
            private ConcurrentDictionary<Tuple<string, string>, string> concurrentCandidate;

            public ConcurrentApriori(List<string> candidates, List<string> transactions, int num_items, int num_transactions,
                                        ConcurrentDictionary<Tuple<string, string>, string> dict, decimal min_sup = 0)
                                        : base(candidates, transactions, num_items, num_transactions, min_sup)
            {
                concurrentCandidate = dict;
            }

            protected override List<string> GenerateCandidates(List<string> candidates, int n)
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
                            string value;
                            Tuple<string, string> pair = new Tuple<string, string>(candidates[i], candidates[j]);
                            if (!concurrentCandidate.TryGetValue(pair, out value))
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
                                    string ret = str_builder.Append(str1).Append(" ").Append(st1[k]).Append(" ").Append(st2[k]).ToString().Trim();
                                    temp_candidates.Add(ret);
                                    concurrentCandidate.GetOrAdd(pair, ret);
                                }
                                else concurrentCandidate.GetOrAdd(pair, "");
                            }
                            else
                            {
                                if (!value.Equals(""))
                                    temp_candidates.Add(value);
                            }
                        }
                    }
                }
                return temp_candidates;
            }
        }

        private const int NUMBER_OF_CHUNKS = 4;
        private decimal m_decimal_min_sup;

        private ConcurrentDictionary<Tuple<string, string>, string> concurrentCandidate;
        private ConcurrentDictionary<string, SupportElement> dictionary;

        public SON(List<string> candidates, List<string> transactions, int num_items, int num_transactions, decimal min_sup = 0)
                    : base(candidates, transactions, num_items, num_transactions, min_sup)
        {
            m_decimal_min_sup = min_sup;
            concurrentCandidate = new ConcurrentDictionary<Tuple<string, string>, string>();
            dictionary = new ConcurrentDictionary<string, SupportElement>();
        }

        public override List<SupportElement> Execute(bool show_message = false)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //split transactions into chunks
            List<List<string>> chunks = SplitList(Transactions, NUMBER_OF_CHUNKS);

            //run threads
            List<Thread> threads = new List<Thread>();
            foreach (List<string> chunk in chunks)
            {
                Thread thread = new Thread(() => PassOne(Candidates, chunk, show_message));
                thread.Start();
                threads.Add(thread);
            }

            //wait for completition
            foreach (Thread thread in threads)
            {
                thread.Join();
            }

            stopwatch.Stop();
            List<SupportElement> output = CalculateFrequentItemsets(dictionary.Values.Select(obj => obj.Label).ToList());

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

        private List<List<string>> SplitList(List<string> list, int chunks)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            var new_list = new List<List<string>>();
            for (int i = 0; i < chunks; i++)
                new_list.Add(new List<string>());
            foreach (string item in list)
            {
                int i = rand.Next(0, chunks);
                new_list[i].Add(item);
            }
            return new_list;
        }

        private void PassOne(List<string> candidates, List<string> transactions, bool show_message = false)
        {
            ConcurrentApriori apriori = new ConcurrentApriori(candidates, transactions, NumItems, transactions.Count, concurrentCandidate, m_decimal_min_sup);
            List<SupportElement> tmp = apriori.Execute();
            foreach (SupportElement element in tmp)
            {
                dictionary.GetOrAdd(element.Label, element);
            }

            if (show_message)
                Console.WriteLine("Thread {0} founded {1} candidates", Thread.CurrentThread.ManagedThreadId, tmp.Count);
        }
    }
}

