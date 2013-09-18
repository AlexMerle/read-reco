using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadReco.Data.Repository
{
	public interface IRepository<T> where T : class 
	{
		T Get(object id);
		IEnumerable<T> GetAll();
	}
}
