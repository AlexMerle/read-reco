using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ServiceModel.Syndication;

namespace ReadReco.IO.Feeds
{
	public class FeedReader
	{
		private string feedUrl = null;

		public FeedReader(string url)
		{
			feedUrl = url;
		}

		public List<FeedItem> Read()
		{
			XmlReader reader = XmlReader.Create(feedUrl);
			SyndicationFeed feed = SyndicationFeed.Load(reader);
			reader.Close();

			List<FeedItem> feedItems = new List<FeedItem>();
			foreach (SyndicationItem item in feed.Items)
			{
				feedItems.Add(new FeedItem(item));
			}

			return feedItems;
		}
	}
}
