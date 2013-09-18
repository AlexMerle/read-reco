using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;

namespace ReadReco.Data.Repository.Mongo
{
	public class MongoContext
	{
		public MongoDatabase Database { get; private set; }
		private MongoServer Server { get; set; }
		private string DatabaseName = "ReadReco";

		public MongoContext() : this("ReadReco")
		{
		}

		public MongoContext(string database)
		{
			DatabaseName = database;

			MongoClient client = new MongoClient(); // connect to localhost
			Server = client.GetServer();
			Database = Server.GetDatabase(DatabaseName);
		}
	}
}
