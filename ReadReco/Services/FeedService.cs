using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.Data.Repository.Mongo;
using ReadReco.Data.Model;
using ReadReco.IO.Feeds;
using ReadReco.Model;

namespace ReadReco.Services
{
	public class FeedService
	{
		public int RefreshFeeds()
		{
			FeedRepository fRep = new FeedRepository();
			List<Feed> feeds = fRep.GetAll().Where(f => f.Active).ToList();

			int addedItems = 0;
			FeedReader reader = new FeedReader();
			foreach (Feed feed in feeds)
			{
				List<FeedItem> feedItems = reader.Read(feed.URL);
				foreach (FeedItem newItem in feedItems)
				{
					if (feed.Items.Where(fi => fi.Id == newItem.Id).Count() == 0)
					{
						feed.Items.Add(newItem);
						addedItems++;
					}
				}
				feed.LastReadTime = DateTime.Now;
				fRep.Update(feed);
			}

			return addedItems;
		}

		public FeedItem GetDocument(string docId)
		{
			FeedRepository fRep = new FeedRepository();
			FeedItem document = fRep.GetDocument(docId);
			return document;
		}

		public List<Feed> GetFeeds()
		{
			FeedRepository fRep = new FeedRepository();
			List<Feed> feeds = fRep.GetAll().Where(f => f.Active).ToList();

			return feeds;
		}

		public List<BagOfWords> GetAllBags()
		{
			List<Feed> feeds = GetFeeds();
			List<BagOfWords> bags = new List<BagOfWords>();
			foreach (Feed feed in feeds)
			{
				foreach (FeedItem doc in feed.Items)
				{
					BagOfWords bag = new BagOfWords(doc.Title, doc.Id, feed.Name);
					bag.AddDocument(doc.Title, doc.ContentText, doc.Tags);
					bags.Add(bag);
				}
			}
			return bags;
		}

		public void ExtractAllLabels()
		{
			FeedRepository fRep = new FeedRepository();
			List<Feed> feeds = fRep.GetAll().Where(f => f.Active).ToList();

			int addedItems = 0;
			foreach (Feed feed in feeds)
			{
				foreach (FeedItem item in feed.Items)
				{
					BagOfWords bag = new BagOfWords();
					bag.AddDocument(item.Title, item.ContentText, item.Tags);

					item.WordsCount = bag.Words.Count();
					item.UniqueWordsCount = bag.WordsFrequency.Count();
					foreach (var freq in bag.Labels)
						item.Labels.Add(new Label(freq.Key, freq.Value.Count, freq.Value.Frequency));

				}
				fRep.Update(feed);
			}
		}

		public BagOfWords AnalyzeFeed(Feed feed)
		{
			BagOfWords bag = new BagOfWords(feed.Name, feed.URL, feed.Name);
			foreach (FeedItem item in feed.Items)
			{
				bag.AddDocument(item.Title, item.ContentText, item.Tags);
			}

			return bag;
		}

		public IEnumerable<BagOfWords> AnalyzeFeedItems(Feed feed)
		{
			List<BagOfWords> bags = new List<BagOfWords>();

			foreach (FeedItem item in feed.Items)
			{
				BagOfWords bag = new BagOfWords(item.Title, item.Id, feed.Name);
				bag.AddDocument(item.Title, item.ContentText, item.Tags);
				yield return bag;
			}
		}

		public List<string> CompareBags(BagOfWords bag1, BagOfWords bag2)
		{
			List<string> commonLabels = new List<string>();
			foreach (string label1 in bag1.Labels.Keys)
			{
				if (bag2.Labels.ContainsKey(label1))
					commonLabels.Add(label1);
			}

			return commonLabels;
		}

		public double CalculateSimilarity(BagOfWords bag1, BagOfWords bag2, bool useLabels)
		{
			Clustering clustering = new Clustering();
			double similarity = clustering.GetCosineDistance(bag1, bag2, useLabels);
			return similarity;
		}
	}
}
