using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.Data.Model;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ReadReco.Data.Repository.Mongo
{
	public class FeedRepository : IRepository<Feed>
	{
		private MongoContext context;

		public FeedRepository() : this(new MongoContext())
		{
		}

		public FeedRepository(MongoContext context)
		{
			this.context = context;
		}

		public Feed Get(object id)
		{
			return null;
		}

		public FeedItem GetDocument(string id)
		{
			MongoCollection<Feed> mongoFeeds = context.Database.GetCollection<Feed>("feeds");
			var query = Query.EQ("Items._id", id);
			Feed feed = mongoFeeds.FindOne(query);
			FeedItem document = feed.Items.Find(fi => fi.Id == id);
			return document;
		}

		public IEnumerable<Feed> GetAll()
		{
			MongoCollection<Feed> mongoFeeds = context.Database.GetCollection<Feed>("feeds");
			foreach (Feed feed in mongoFeeds.FindAll())
				yield return feed;
		}

		public void Add(Feed feed)
		{
			MongoCollection<Feed> mongoFeeds = context.Database.GetCollection<Feed>("feeds");
			mongoFeeds.Insert(feed);
		}

		public void Update(Feed feed)
		{
			MongoCollection<Feed> mongoFeeds = context.Database.GetCollection<Feed>("feeds");
			mongoFeeds.Save(feed);
		}
	}
}
