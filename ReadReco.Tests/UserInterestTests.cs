using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ReadReco.Services;
using ReadReco.Data.Model;
using ReadReco.Model;
using System.IO;

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
		public void TestTeaching()
		{
			string userName = "user1";

			List<BagOfWords> bags = feedService.GetAllBags();
			
			UserInterest interest = userService.GetUser(userName);
			if (interest == null)
			{
				interest = userService.AddUser(userName);
				
				FeedItem document = feedService.GetDocument("tag:blogger.com,1999:blog-19732346.post-6636802898282623833");
				userService.LikeDocument(interest, document);
				document = feedService.GetDocument("tag:blogger.com,1999:blog-19732346.post-1441194024531049182");
				userService.LikeDocument(interest, document);
			}

			BagOfWords userBag = new BagOfWords(interest.Id, interest);
			interest.Ratings = userBag.CalculateRatings(bags).Where(rating => rating.Rating > 0).ToList();
			userService.UpdateUser(interest);

			//userService.RemoveUser(userName);
		}

		[Test]
		public void TestTeachingAndSoccer()
		{
			string userName = "user2";

			List<BagOfWords> bags = feedService.GetAllBags();

			UserInterest interest = userService.GetUser(userName);
			if (interest == null)
			{
				interest = userService.AddUser(userName);

				FeedItem document = feedService.GetDocument("tag:blogger.com,1999:blog-19732346.post-6636802898282623833");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-19732346.post-1441194024531049182");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-1441001597549095809");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-3455512636884019098");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-8932640771092230018");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-6976607004104902900");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-2476250807373693629");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-6800302380861549232");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-4232728372957388206");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-6199830262183471515");
				userService.LikeDocument(interest, document);
			}

			BagOfWords userBag = new BagOfWords(interest.Id, interest);
			interest.Ratings = userBag.CalculateRatings(bags).Where(rating => rating.Rating > 0).ToList();
			userService.UpdateUser(interest);
		}

		[Test]
		public void TestSoccer()
		{
			string userName = "user3";

			List<BagOfWords> bags = feedService.GetAllBags();

			UserInterest interest = userService.GetUser(userName);
			if (interest == null)
			{
				interest = userService.AddUser(userName);

				FeedItem document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-1441001597549095809");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-3455512636884019098");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-8932640771092230018");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-6976607004104902900");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-2476250807373693629");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-6800302380861549232");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-4232728372957388206");
				userService.LikeDocument(interest, document);

				document = feedService.GetDocument("tag:blogger.com,1999:blog-468745685421226335.post-6199830262183471515");
				userService.LikeDocument(interest, document);
			}

			BagOfWords userBag = new BagOfWords(interest.Id, interest);
			interest.Ratings = userBag.CalculateRatings(bags).Where(rating => rating.Rating > 0).ToList();
			userService.UpdateUser(interest);
		}

		private List<DocumentRating> CalculateAndSaveResults(UserInterest interest, List<BagOfWords> bags, string filename)
		{
			BagOfWords userBag = new BagOfWords(interest.Id, interest);
			var ratings = userBag.CalculateRatings(bags);

			StreamWriter file = new System.IO.StreamWriter(filename);
			foreach (var result in ratings.Take(50))
				file.WriteLine("{0}\t - {1}\t - {2:F3}", result.Type, result.Name, result.Rating);

			file.Close();

			return ratings;
		}

		private void DumpInterests(UserInterest interest, string filename)
		{
			List<Label> sortedList = interest.Labels;
			sortedList.Sort((firstLabel, nextLabel) =>
				{
					return -1 * firstLabel.Frequency.CompareTo(nextLabel.Frequency);
				}
			);

			StreamWriter file = new System.IO.StreamWriter(filename);
			foreach (var result in sortedList)
				file.WriteLine("{0:F3}\t - {1}", result.Count, result.Name);

			file.Close();
		}
	}
}
