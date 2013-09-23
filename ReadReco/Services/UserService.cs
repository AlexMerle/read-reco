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
		public UserInterest GetUser(string name)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			return uiRep.Get(name);
		}

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

		public UserInterest GetOrAddUser(string name)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			UserInterest interest = uiRep.Get(name);
			if (interest == null)
			{
				interest = new UserInterest
				{
					Id = name
				};
				uiRep.Add(interest);
			}
			return interest;
		}

		public void UpdateUser(UserInterest interest)
		{
			UserInterestRepository uiRep = new UserInterestRepository();
			uiRep.Update(interest);
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
