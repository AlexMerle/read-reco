using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.IO.Feeds;
using ReadReco.Model;
using ReadReco.Data.Model;
using ReadReco.Services;

namespace ReadReco
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.Out.WriteLine("You need to enter a command in arguments");
				return;
			}

			DataService dataService = new DataService();
			FeedService feedService = new FeedService();
			UserService userService = new UserService();
			switch (args[0])
			{
				case "-addfeed":
					if (args.Length != 3)
						Console.Out.WriteLine("Incorrect number of parameters");

					bool result = dataService.AddFeed(args[1], args[2]);
					Console.Out.WriteLine("New feed successfully added");
					break;

				case "-refreshfeeds":	
					int feedsCount = feedService.RefreshFeeds();
					Console.Out.WriteLine("{0} new feed items added", feedsCount);
					break;

				case "-getlabels":
					feedService.ExtractAllLabels();
					Console.Out.WriteLine("Labels are successfully extracted");
					break;

				case "-testfeeds":
					TestFeeds(feedService);
					Console.In.ReadLine();
					break;

				case "-testfeeditems":
					TestFeedItems(feedService);
					Console.In.ReadLine();
					break;

				case "-adduser":
					if (args.Length != 2)
						Console.Out.WriteLine("Incorrect number of parameters");
					userService.AddUser(args[1]);
					Console.Out.WriteLine("New user successfully added");
					
					break;

				case "-likedoc":
					if (args.Length != 3)
						Console.Out.WriteLine("Incorrect number of parameters");
					FeedItem document = feedService.GetDocument(args[2]);
					if (document == null)
						Console.Out.WriteLine("Document was not found");

					userService.LikeDocument(args[1], document);
					Console.Out.WriteLine("Document was successfully liked");
					break;
			}

			//TestFeeds();
			//TestFeedItems();

			//Console.In.ReadLine();
		}

		private static void TestFeeds(FeedService feedService)
		{
			List<Feed> feeds = feedService.GetFeeds();

			BagOfWords bag1 = feedService.AnalyzeFeed(feeds[0]);
			BagOfWords bag2 = feedService.AnalyzeFeed(feeds[1]);
			BagOfWords bag3 = feedService.AnalyzeFeed(feeds[2]);
			BagOfWords bag4 = feedService.AnalyzeFeed(feeds[3]);

			List<string> commonLabels = feedService.CompareBags(bag1, bag2);
			OutputCommonLabels(bag1, bag2, commonLabels);

			commonLabels = feedService.CompareBags(bag1, bag3);
			OutputCommonLabels(bag1, bag3, commonLabels);

			commonLabels = feedService.CompareBags(bag1, bag4);
			OutputCommonLabels(bag1, bag4, commonLabels);

			commonLabels = feedService.CompareBags(bag2, bag3);
			OutputCommonLabels(bag2, bag3, commonLabels);

			commonLabels = feedService.CompareBags(bag2, bag4);
			OutputCommonLabels(bag2, bag4, commonLabels);

			commonLabels = feedService.CompareBags(bag3, bag4);
			OutputCommonLabels(bag3, bag4, commonLabels);

			double similarity = feedService.CalculateSimilarity(bag1, bag2, true);
			OutputSimilarity(bag1, bag2, similarity);

			similarity = feedService.CalculateSimilarity(bag1, bag3, true);
			OutputSimilarity(bag1, bag3, similarity);

			similarity = feedService.CalculateSimilarity(bag1, bag4, true);
			OutputSimilarity(bag1, bag4, similarity);

			similarity = feedService.CalculateSimilarity(bag2, bag3, true);
			OutputSimilarity(bag2, bag3, similarity);

			similarity = feedService.CalculateSimilarity(bag2, bag4, true);
			OutputSimilarity(bag2, bag4, similarity);

			similarity = feedService.CalculateSimilarity(bag3, bag4, true);
			OutputSimilarity(bag3, bag4, similarity);

			//Clustering cluster = new Clustering();
			//cluster.Clusterize(items);
		}

		private static void TestFeedItems(FeedService feedService)
		{
			List<Feed> feeds = feedService.GetFeeds();

			List<BagOfWords> bags = new List<BagOfWords>();
			foreach (Feed feed in feeds)
				bags.AddRange(feedService.AnalyzeFeedItems(feed));

			Clustering clustering = new Clustering();
			double[,] similars = new double[bags.Count, bags.Count];
			var docSimilars = new Dictionary<Tuple<BagOfWords, BagOfWords>, double>();
			for (int i = 0; i < bags.Count - 1; i++)
			{
				for (int j = i + 1; j < bags.Count; j++)
				{
					if (bags[i].Type == bags[j].Type)
						continue;
					double sim = clustering.GetCosineDistance(bags[i], bags[j], true);
					similars[i, j] = sim;
					docSimilars.Add(new Tuple<BagOfWords, BagOfWords>(bags[i], bags[j]), sim);
				}
			}

			Console.In.ReadLine();

			OutputSimilarityMatrix(docSimilars);
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

		private static void OutputSimilarityMatrix(Dictionary<Tuple<BagOfWords, BagOfWords>, double> docSimilars)
		{
			List<KeyValuePair<Tuple<BagOfWords, BagOfWords>, double>> sortedList = docSimilars.ToList();

			sortedList.Sort((firstPair, nextPair) =>
				{
					return firstPair.Value.CompareTo(nextPair.Value);
				}
			);

			foreach (var sim in sortedList.Skip(sortedList.Count - 2))
			{
				Console.Out.WriteLine("Similarity for documents '{0}' and '{1}':", sim.Key.Item1.Name, sim.Key.Item2.Name);
				Console.Out.WriteLine("- similarity: {0}", sim.Value);
				Console.Out.WriteLine("---------------------");
				Console.Out.WriteLine();
			}
		}
	}
}
