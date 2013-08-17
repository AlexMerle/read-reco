using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.IO.Feeds;

namespace ReadReco
{
	class Program
	{
		static void Main(string[] args)
		{
			FeedReader reader = new FeedReader("http://www.lonelyplanet.com/blog/feed/atom/");
			List<FeedItem> items = reader.Read();
		}
	}
}
