using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReadReco.Data.Model
{
	public class UserInterest
	{
		public string Id { get; set; }
		public List<string> LikedDocs { get; set; }
		public List<Label> Labels { get; set; }
		public int WordsCount { get; set; }

		public UserInterest()
		{
			LikedDocs = new List<string>();
			Labels = new List<Label>();
		}

		public void AddDocument(FeedItem doc)
		{
			// merge new labels into existing labels
			foreach (Label newLabel in doc.Labels)
			{
				Label duplLabel = Labels.Find(lbl => lbl.Name == newLabel.Name);
				if (duplLabel != null)
				{
					duplLabel.Count += newLabel.Count;
					duplLabel.Frequency = 0; // reset
				}
				else
				{
					Labels.Add(newLabel);
					newLabel.Frequency = 0; // reset
				}
			}

			// recalculate frequencies
			WordsCount += doc.WordsCount;
			foreach (Label label in Labels)
			{
				label.Frequency = (double)label.Count / WordsCount;
			}

			LikedDocs.Add(doc.Id);
		}
	}
}
