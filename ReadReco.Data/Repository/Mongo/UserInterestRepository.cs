using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.Data.Model;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ReadReco.Data.Repository.Mongo
{
	public class UserInterestRepository : IRepository<UserInterest>
	{
		private MongoContext context;

		public UserInterestRepository() : this(new MongoContext())
		{
		}

		public UserInterestRepository(MongoContext context)
		{
			this.context = context;
		}

		public UserInterest Get(object id)
		{
			MongoCollection<UserInterest> mongoInterests = context.Database.GetCollection<UserInterest>("userInterests");
			var query = Query.EQ("_id", id.ToString());
			UserInterest interest = mongoInterests.FindOne(query);
			return interest;
		}

		public IEnumerable<UserInterest> GetAll()
		{
			MongoCollection<UserInterest> mongoInterests = context.Database.GetCollection<UserInterest>("userInterests");
			foreach (UserInterest interest in mongoInterests.FindAll())
				yield return interest;
		}

		public void Add(UserInterest interest)
		{
			MongoCollection<UserInterest> mongoInterests = context.Database.GetCollection<UserInterest>("userInterests");
			mongoInterests.Insert(interest);
		}

		public void Update(UserInterest interest)
		{
			MongoCollection<UserInterest> mongoInterests = context.Database.GetCollection<UserInterest>("userInterests");
			mongoInterests.Save(interest);
		}

		public void Remove(string id)
		{
			MongoCollection<UserInterest> mongoInterests = context.Database.GetCollection<UserInterest>("userInterests");
			var query = Query.EQ("_id", id);
			mongoInterests.Remove(query);
		}
	}
}
