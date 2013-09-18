using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ReadReco.Services;
using ReadReco.Data.Model;
using ReadReco.Model;

namespace ReadReco.Tests
{
	[TestFixture]
	public class UserInterestTests
	{
		DataService dataService;
		FeedService feedService;
		UserService userService;

		[SetUp]
		public void Init()
		{
			dataService = new DataService();
			feedService = new FeedService();
			userService = new UserService();
		}

		[Test]
		public void Test()
		{
			string userName = "user1";
			
			UserInterest interest = userService.AddUser(userName);
			FeedItem document = feedService.GetDocument("tag:blogger.com,1999:blog-19732346.post-378949587116144446");
			userService.LikeDocument(interest, document);
			document = feedService.GetDocument("tag:blogger.com,1999:blog-19732346.post-1441194024531049182");
			userService.LikeDocument(interest, document);

			List<Feed> feeds = feedService.GetFeeds();
			List<BagOfWords> bags = new List<BagOfWords>();
			foreach (Feed feed in feeds)
			{
				foreach (FeedItem doc in feed.Items)
				{
					BagOfWords bag = new BagOfWords(doc.Title, feed.URL);
					bag.AddDocument(doc.Title, doc.ContentText, doc.Tags);
					bags.Add(bag);
				}
			}

			BagOfWords userBag = new BagOfWords(interest.Id, interest);

			Clustering clustering = new Clustering();
			var docSimilars = new Dictionary<BagOfWords, double>();
			for (int i = 0; i < bags.Count - 1; i++)
			{
				double sim = clustering.GetCosineDistance(userBag, bags[i], true);
				docSimilars.Add(bags[i], sim);
			}

			List<KeyValuePair<BagOfWords, double>> sortedList = docSimilars.ToList();
			sortedList.Sort((firstPair, nextPair) =>
				{
					return -1 * firstPair.Value.CompareTo(nextPair.Value);
				}
			);

			userService.RemoveUser(userName);
		}

	}
}
