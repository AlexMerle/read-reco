using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.IO.Feeds;
using ReadReco.Model;

namespace ReadReco
{
	class Program
	{
		static void Main(string[] args)
		{
			//TestFeeds();
			TestFeedItems();

			Console.In.ReadLine();
		}

		private static void TestFeeds()
		{
			FeedReader reader = new FeedReader();

			BagOfWords bag1 = AnalyzeFeed(reader, "http://www.lonelyplanet.com/blog/feed/atom/", "Lonely Planet");
			BagOfWords bag2 = AnalyzeFeed(reader, "http://weblogs.asp.net/scottgu/atom.aspx", "Scott Gu");
			BagOfWords bag3 = AnalyzeFeed(reader, "http://blogs.msdn.com/b/adonet/rss.aspx", "ADO.NET");
			BagOfWords bag4 = AnalyzeFeed(reader, "http://neopythonic.blogspot.com/feeds/posts/default", "Python");

			CompareBags(bag1, bag2);
			CompareBags(bag1, bag3);
			CompareBags(bag1, bag4);
			CompareBags(bag2, bag3);
			CompareBags(bag2, bag4);
			CompareBags(bag3, bag4);

			CalculateSimilarity(bag1, bag2, true);
			//CalculateSimilarity(bag1, bag2, false);

			CalculateSimilarity(bag1, bag3, true);
			//CalculateSimilarity(bag1, bag3, false);

			CalculateSimilarity(bag1, bag4, true);
			//CalculateSimilarity(bag1, bag4, false);

			CalculateSimilarity(bag2, bag3, true);
			//CalculateSimilarity(bag2, bag3, false);

			CalculateSimilarity(bag2, bag4, true);
			//CalculateSimilarity(bag2, bag4, false);

			CalculateSimilarity(bag3, bag4, true);
			//CalculateSimilarity(bag3, bag4, false);

			//Clustering cluster = new Clustering();
			//cluster.Clusterize(items);
		}

		private static void TestFeedItems()
		{
			FeedReader reader = new FeedReader();

			List<BagOfWords> bags = new List<BagOfWords>();
			bags.AddRange(AnalyzeFeedItems(reader, "http://www.lonelyplanet.com/blog/feed/atom/"));
			bags.AddRange(AnalyzeFeedItems(reader, "http://weblogs.asp.net/scottgu/atom.aspx"));
			bags.AddRange(AnalyzeFeedItems(reader, "http://blogs.msdn.com/b/adonet/rss.aspx"));
			bags.AddRange(AnalyzeFeedItems(reader, "http://neopythonic.blogspot.com/feeds/posts/default"));

			Clustering clustering = new Clustering();
			double[,] similars = new double[bags.Count, bags.Count];
			Dictionary<Tuple<string, string>, double> docSimilars = new Dictionary<Tuple<string, string>, double>();
			for (int i = 0; i < bags.Count-1; i++)
			{
				for (int j = i + 1; j < bags.Count; j++)
				{
					if (bags[i].Type == bags[j].Type)
						continue;
					double sim = clustering.GetCosineDistance(bags[i], bags[j], true);
					similars[i, j] = sim;
					docSimilars.Add(new Tuple<string, string>(bags[i].Name, bags[j].Name), sim);
				}
			}

			OutputSimilarityMatrix(docSimilars);
		}

		private static BagOfWords AnalyzeFeed(FeedReader reader, string feedUrl, string name)
		{
			List<FeedItem> feedItems = reader.Read(feedUrl);
			
			BagOfWords bag = new BagOfWords(name, feedUrl);
			foreach (FeedItem item in feedItems)
			{
				bag.AddDocument(item.ContentText);
			}

			OutputStatistics(bag);

			return bag;
		}

		private static IEnumerable<BagOfWords> AnalyzeFeedItems(FeedReader reader, string feedUrl)
		{
			List<FeedItem> feedItems = reader.Read(feedUrl);
			List<BagOfWords> bags = new List<BagOfWords>();

			foreach (FeedItem item in feedItems)
			{
				BagOfWords bag = new BagOfWords(item.Title, feedUrl);
				bag.AddDocument(item.ContentText);
				yield return bag;
			}
		}

		private static void CompareBags(BagOfWords bag1, BagOfWords bag2)
		{
			List<string> commonLabels = new List<string>();
			foreach (string label1 in bag1.Labels.Keys)
			{
				if (bag2.Labels.ContainsKey(label1))
					commonLabels.Add(label1);
			}

			OutputCommonLabels(bag1, bag2, commonLabels);
		}

		private static void CalculateSimilarity(BagOfWords bag1, BagOfWords bag2, bool useLabels)
		{
			Clustering clustering = new Clustering();
			double similarity = clustering.GetCosineDistance(bag1, bag2, useLabels);

			OutputSimilarity(bag1, bag2, similarity);
		}

		private static void OutputStatistics(BagOfWords bag)
		{
			Console.Out.WriteLine("Statistics for document '{0}':", bag.Name);
			Console.Out.WriteLine("- number of docs: {0}", bag.DocumentsCount);
			Console.Out.WriteLine("- number of words: {0}", bag.Words.Count);
			Console.Out.WriteLine("- number of stems: {0}", bag.WordsFrequency.Count);
			Console.Out.WriteLine("- number of labels: {0}", bag.Labels.Count);

			Console.Out.WriteLine("- list of labels:");
			var bagSorted = bag.GetSortedLabels();
			foreach (KeyValuePair<string, BagOfWords.WordFrequency> word in bagSorted)
			{
				Console.Out.WriteLine("\t{0}\t\t- {1} - {2}", word.Key, word.Value.Count, word.Value.Frequency);
			}
			Console.Out.WriteLine("---------------------");
			Console.Out.WriteLine();
		}

		private static void OutputCommonLabels(BagOfWords bag1, BagOfWords bag2, List<string> commonLabels)
		{
			Console.Out.WriteLine("Common labels for documents '{0}' and '{1}':", bag1.Name, bag2.Name);
			Console.Out.WriteLine("- number of common labels: {0}", commonLabels.Count);

			Console.Out.WriteLine("- list of labels:");
			foreach (string label in commonLabels)
			{
				Console.Out.WriteLine("\t{0}", label);
			}
			Console.Out.WriteLine("---------------------");
			Console.Out.WriteLine();
		}

		private static void OutputSimilarity(BagOfWords bag1, BagOfWords bag2, double similarity)
		{
			Console.Out.WriteLine("Similarity for documents '{0}' and '{1}':", bag1.Name, bag2.Name);
			Console.Out.WriteLine("- similarity: {0}", similarity);
			Console.Out.WriteLine("---------------------");
			Console.Out.WriteLine();
		}

		private static void OutputSimilarityMatrix(Dictionary<Tuple<string, string>, double> docSimilars)
		{
			List<KeyValuePair<Tuple<string, string>, double>> sortedList = docSimilars.ToList();

			sortedList.Sort((firstPair, nextPair) =>
				{
					return firstPair.Value.CompareTo(nextPair.Value);
				}
			);

			foreach (var sim in sortedList)
			{
				Console.Out.WriteLine("Similarity for documents '{0}' and '{1}':", sim.Key.Item1, sim.Key.Item2);
				Console.Out.WriteLine("- similarity: {0}", sim.Value);
				Console.Out.WriteLine("---------------------");
				Console.Out.WriteLine();
			}
		}
	}
}
