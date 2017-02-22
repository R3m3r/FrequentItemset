using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrequentItemset;

public class Program
{
    static void Main(string[] args)
    {
        string trans_file = "transa.txt";
        List<string> candidates = new List<string>();
        List<string> transactions = new List<string>();

        Console.WriteLine("Default Configuration: ");
        Console.WriteLine("\tRegular transaction file with ' ' item separator.");
        Console.WriteLine("\tTransa File: " + trans_file);

        Console.Write("Enter transaction filename (return for '" + trans_file + "'): ");
        string input = Console.ReadLine();
        if (!input.Equals("", StringComparison.OrdinalIgnoreCase))
            trans_file = input;

        // number of transactions
        Console.Write("Enter number of transactions: ");
        int num_transactions = int.Parse(Console.ReadLine());

        // minsup
        Console.Write("Enter min sup: ");
        decimal min_sup = decimal.Parse(Console.ReadLine());

        try
        {
            string line;
            StreamReader data_in = new StreamReader(trans_file);
            while ((line = data_in.ReadLine()) != null)
            {
                transactions.Add(line);
                string[] st_file = line.Split(' ');
                foreach (string element in st_file)
                {
                    if (!candidates.Contains(element))
                        candidates.Add(element);
                }
            }
            data_in.Close();
        }
        catch (IOException e)
        {
            Console.WriteLine(e);
        }

        if (num_transactions > transactions.Count)
            num_transactions = transactions.Count;

        int j = transactions.Count - num_transactions;
        for (int i = 0; i < j; i++)
        {
            transactions.Remove(transactions[num_transactions - 1]);
        }

        Apriori apriori = new Apriori(candidates, transactions, candidates.Count, num_transactions, min_sup);
        PCY pcy = new PCY(candidates, transactions, candidates.Count, num_transactions, min_sup);
        Sampling sampling = new Sampling(candidates, transactions, candidates.Count, num_transactions, min_sup);
        Toivonen toivonen = new Toivonen(candidates, transactions, candidates.Count, num_transactions, min_sup);
        SON son = new SON(candidates, transactions, candidates.Count, num_transactions, min_sup);

        List<BaseAlgorithm> algorithms = new List<BaseAlgorithm>();
        algorithms.AddRange(new BaseAlgorithm[] { apriori, pcy, sampling, son, toivonen });
        foreach (BaseAlgorithm alg in algorithms)
        {
            Console.WriteLine("\n{0} algorithm has started.", alg.GetType());
            Console.WriteLine("Input configuration: {0} items, {1} transactions, minsup = {2}\n", alg.NumItems, alg.NumTransactions, alg.MinSup);
            List<SupportElement> output = alg.Execute(true);
            alg.WriteOutputToFile(alg.GetType() + "_output.txt", output);
        }
    }

    static double Jaccard(string first_file, string second_file)
    {
        List<string> Apriori = new List<string>();
        List<string> Sampling = new List<string>();

        string line;
        // leggo l'insieme apriori
        try
        {
            StreamReader data_in = new StreamReader(first_file);
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
            StreamReader data_in = new StreamReader(second_file);
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
        double sim = Intersect.Count / (double)Union.Count;

        return sim;
    }
}
