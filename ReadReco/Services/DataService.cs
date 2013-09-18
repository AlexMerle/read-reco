using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.Data.Repository.Mongo;
using ReadReco.Data.Model;

namespace ReadReco.Services
{
	public class DataService
	{
		public bool AddFeed(string feedUrl, string feedName)
		{
			FeedRepository rep = new FeedRepository();
			Feed feed = new Feed {
				URL = feedUrl,
				Name = feedName,
				Active = true,
				LastReadTime = null
			};
			try {
				rep.Add(feed);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
}
