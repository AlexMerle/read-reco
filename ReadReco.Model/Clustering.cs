using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.Carrot2.Core;
using ReadReco.IO.Feeds;

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
	}
}
