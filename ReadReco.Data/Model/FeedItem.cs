using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadReco.Data.Model
{
	public class FeedItem // TODO: rename to Document
	{
		public string Id { get; set; }
		public string Title { get; set; }
		public string ContentText { get; set; }
		public List<string> Tags { get; set; }
		public List<Label> Labels { get; set; }
		public int UniqueWordsCount { get; set; }
		public int WordsCount { get; set; }

		public FeedItem()
		{
			Tags = new List<string>();
			Labels = new List<Label>();
		}
	}
}
