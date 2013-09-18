using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ReadReco.Data.Repository.Mongo;
using ReadReco.Data.Model;

namespace ReadReco.Services
{
	public class UserService
	{
		public UserInterest AddUser(string name)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			UserInterest interest = new UserInterest
			{
				Id = name
			};
			uiRep.Add(interest);
			
			return interest;
		}

		public void RemoveUser(string name)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			uiRep.Remove(name);
		}

		public void LikeDocument(string name, FeedItem document)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			UserInterest interest = uiRep.Get(name);
			LikeDocument(interest, document);
		}

		public void LikeDocument(UserInterest interest, FeedItem document)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			interest.AddDocument(document);
			uiRep.Update(interest);
		}
	}
}
