using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;

namespace ReadReco.Data.Model
{
	public class Feed
	{
		public ObjectId Id { get; set; }
		public string URL { get; set; }
		public string Name { get; set; }
		public DateTime? LastReadTime { get; set; }
		public bool Active { get; set; }
		public List<FeedItem> Items { get; set; }

		public Feed()
		{
			Items = new List<FeedItem>();
		}
	}
}
