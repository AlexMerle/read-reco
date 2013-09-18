using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Net;
using HtmlAgilityPack;
using ReadReco.Data.Model;

namespace ReadReco.IO.Feeds
{
	public class FeedReader
	{
		public List<FeedItem> Read(string feedUrl)
		{
			XmlReader reader = XmlReader.Create(feedUrl);
			SyndicationFeed feed = SyndicationFeed.Load(reader);
			reader.Close();

			List<FeedItem> feedItems = new List<FeedItem>();
			foreach (SyndicationItem item in feed.Items)
			{
				feedItems.Add(ParseFeedItem(item));
			}

			return feedItems;
		}

		public FeedItem ParseFeedItem(SyndicationItem item)
		{
			FeedItem feedItem = new FeedItem();
			feedItem.Id = item.Id;
			feedItem.Title = WebUtility.HtmlDecode(item.Title.Text);

			SyndicationContent content = item.Content ?? item.Summary;
			if (!(content is TextSyndicationContent))
				feedItem.ContentText = string.Empty;

			TextSyndicationContent textContent = (TextSyndicationContent)content;
			if (textContent.Type.ToLower() == "html")
			{
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(textContent.Text);
				feedItem.ContentText = doc.DocumentNode.InnerText;
			}
			else
			{
				try
				{
					HtmlDocument doc = new HtmlDocument();
					doc.LoadHtml(textContent.Text);
					feedItem.ContentText = doc.DocumentNode.InnerText;
				}
				catch
				{
					feedItem.ContentText = textContent.Text;
				}
			}

			feedItem.ContentText = WebUtility.HtmlDecode(feedItem.ContentText.Replace("\n", " "));

			feedItem.Tags = new List<string>();
			foreach (var cat in item.Categories)
				feedItem.Tags.Add(cat.Name);

			return feedItem;
		} 
	}
}
