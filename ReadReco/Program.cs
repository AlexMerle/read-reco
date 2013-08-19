using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.IO.Feeds;
using ReadReco.Model;

namespace ReadReco
{
	class Program
	{
		static void Main(string[] args)
		{
			FeedReader reader = new FeedReader();
			List<FeedItem> items = reader.Read("http://www.lonelyplanet.com/blog/feed/atom/");
			items.AddRange(reader.Read("http://weblogs.asp.net/scottgu/atom.aspx"));

			Clustering cluster = new Clustering();
			cluster.Clusterize(items);
		}
	}
}
