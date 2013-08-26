using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Syndication;
using HtmlAgilityPack;
using System.Net;

namespace ReadReco.IO.Feeds
{
	public class FeedItem
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string ContentText { get; set; }
		public List<string> Tags { get; set; }

		public FeedItem()
		{
			Tags = new List<string>();
		}

		public FeedItem(SyndicationItem item)
		{
			Id = item.Id;
			Title = WebUtility.HtmlDecode(item.Title.Text);

			SyndicationContent content = item.Content ?? item.Summary;
			if (!(content is TextSyndicationContent))
				ContentText = string.Empty;

			TextSyndicationContent textContent = (TextSyndicationContent)content;
			if (textContent.Type.ToLower() == "html")
			{
				HtmlDocument doc = new HtmlDocument();
				doc.LoadHtml(textContent.Text);
				ContentText = doc.DocumentNode.InnerText;
			}
			else
			{
				try
				{
					HtmlDocument doc = new HtmlDocument();
					doc.LoadHtml(textContent.Text);
					ContentText = doc.DocumentNode.InnerText;
				}
				catch
				{
					ContentText = textContent.Text;
				}
			}

			ContentText = WebUtility.HtmlDecode(ContentText.Replace("\n", " "));

			Tags = new List<string>();
			foreach (var cat in item.Categories)
				Tags.Add(cat.Name);
		} 
	}
}
