using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.Carrot2.Core;
using ReadReco.IO.Feeds;
using ReadReco.Data.Model;

namespace ReadReco.Model
{
	public class Clustering
	{
		public void Clusterize(List<FeedItem> feeds)
		{
			List<Document> documents = feeds.Select<FeedItem, Document>(feed => new Document(feed.Title, feed.ContentText)).ToList();
			using (var controller = ControllerFactory.CreateSimple())
			{
				var attributes = new Dictionary<string, object>();
				attributes[AttributeNames.Documents] = documents;
				var result = controller.Process(attributes, Algorithms.List.Lingo());
			}
		}

		public double GetCosineDistance(BagOfWords bag1, BagOfWords bag2, bool useLabels)
		{
			var words1 = useLabels ? bag1.Labels : bag1.WordsFrequency;
			var words2 = useLabels ? bag2.Labels : bag2.WordsFrequency;

			double numerator = 0;
			double denumerator1 = 0;
			foreach (var word1 in words1)
			{
				if (words2.ContainsKey(word1.Key))
					numerator += word1.Value.Frequency * words2[word1.Key].Frequency;

				denumerator1 += word1.Value.Frequency * word1.Value.Frequency;
			}
			denumerator1 = Math.Sqrt(denumerator1);

			double denumerator2 = 0;
			foreach (var word in words2)
				denumerator2 += word.Value.Frequency * word.Value.Frequency;
			denumerator2 = Math.Sqrt(denumerator2);

			double result = numerator / (denumerator1 * denumerator2);
			return result;
		}

		private List<KeyValuePair<string, BagOfWords.WordFrequency>> GetSortedDictionary(Dictionary<string, BagOfWords.WordFrequency> dict)
		{
			List<KeyValuePair<string, BagOfWords.WordFrequency>> sortedList = dict.ToList();

			sortedList.Sort((firstPair, nextPair) =>
				{
					return firstPair.Value.Count.CompareTo(nextPair.Value.Count);
				}
			);

			return sortedList;
		}
	}
}
