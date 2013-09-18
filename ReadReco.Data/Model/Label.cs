using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadReco.Data.Model
{
	public class Label
	{
		public string Name { get; set; }
		public int Count { get; set; }
		public double Frequency { get; set; }

		public Label(string name, int count, double frequency)
		{
			Name = name;
			Count = count;
			Frequency = frequency;
		}
	}
}
