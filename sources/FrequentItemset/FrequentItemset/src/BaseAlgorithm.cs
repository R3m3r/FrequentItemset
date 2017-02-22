using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrequentItemset
{
    public abstract class BaseAlgorithm
    {
        protected List<string> m_candidates;
        protected List<string> m_transactions;

        protected int m_num_items;
        protected int m_num_transactions;
        protected int m_min_sup;

        public List<string> Candidates
        {
            get { return m_candidates; }
        }

        public List<string> Transactions
        {
            get { return m_transactions; }
        }

        public int NumItems
        {
            get { return m_num_items; }
        }

        public int NumTransactions
        {
            get { return m_num_transactions; }
        }

        public int MinSup
        {
            get { return m_min_sup; }
        }

        public BaseAlgorithm(List<string> candidates, List<string> transactions, int num_items, int num_transactions, decimal min_sup = 0)
        {
            m_candidates = candidates;
            m_transactions = transactions;
            m_num_items = num_items;
            m_num_transactions = num_transactions;
            m_min_sup = (int)(min_sup * num_transactions);
        }

        public void WriteOutputToFile(string filename, List<SupportElement> output)
        {
            try
            {
                foreach (SupportElement element in output)
                {
                    List<string> str_numbers = element.Label.Split(' ').ToList();
                    List<int> int_numbers = new List<int>();
                    int number;
                    foreach (string str in str_numbers)
                    {
                        if (int.TryParse(str, out number))
                            int_numbers.Add(number);
                    }
                    int_numbers.Sort();
                    element.Label = "";
                    foreach (int num in int_numbers)
                        element.Label += string.Format("{0} ", num);
                    element.Label = element.Label.TrimEnd();
                }

                List<SupportElement> ordered_list = output.OrderByDescending(obj => obj.Label).ToList();

                StreamWriter data_out = new StreamWriter(filename);
                data_out.Write("Input configuration: {0} items, {1} transactions, minsup = {2}\n", NumItems, NumTransactions, MinSup);
                data_out.Write("Frequent itemsets: {0}\n", output.Count);
                data_out.Write(string.Join("\n", ordered_list));
                data_out.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e);
            }
        }

        public abstract List<SupportElement> Execute(bool show_message = false);
    }
}

