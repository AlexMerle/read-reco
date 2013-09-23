using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveonik.Stemmers;
using ReadReco.IO.Feeds;
using System.Reflection;
using System.IO;
using ReadReco.Data.Model;

namespace ReadReco.Model
{
	public class BagOfWords
	{
		public string Name { get; private set; }
		public string DocumentID { get; private set; }
		public string Type { get; private set; }
		private UserInterest Interest { get; set; }

		public List<string> Words { get; private set; }
		public Dictionary<string, string> StemmedWords { get; private set; }
		public Dictionary<string, WordFrequency> WordsFrequency { get; private set; }
		public Dictionary<string, WordFrequency> Labels { get; private set; }
		public int DocumentsCount { get; private set; }

		private HashSet<string> stopWords = new HashSet<string>();
		private Tokenizer tokenizer;
		private const double LabelThreshold = 0.2; // 20%
		private const int TitleWeight = 2;
		private const int TagWeight = 2;
		
		public BagOfWords()
		{
			Words = new List<string>();
			StemmedWords = new Dictionary<string, string>();
			WordsFrequency = new Dictionary<string, WordFrequency>();
			Labels = new Dictionary<string, WordFrequency>();

			tokenizer = InitializeTokenizer();
			ReadStopWords();
		}

		public BagOfWords(string name, string documentId, string type) : this()
		{
			Name = name;
			Type = type;
			DocumentID = documentId;
		}

		public BagOfWords(string name, UserInterest interest) : this()
		{
			Name = name;
			Interest = interest;
			foreach (Label label in interest.Labels)
				Labels.Add(label.Name, new WordFrequency() { Count = label.Count, Frequency = label.Frequency });
		}

		public void AddDocument(string title, string document, List<string> tags)
		{
			StringBuilder text = new StringBuilder();
			for (int i = 0; i < TitleWeight; i++)
				text.AppendLine(title);

			foreach (string tag in tags)
				for (int i = 0; i < TagWeight; i++)
					text.AppendLine(tag);

			text.AppendLine(document);

			Tokenizer.TokenList words = tokenizer.Tokenize(NormalizeText(text.ToString()));

			IStemmer stemmer = new EnglishStemmer();
			foreach (Tokenizer.Token token in words.Where(w => w.Type == Tokenizer.TokenType.Identifier))
			{
				string loweredTokenText = token.Text.Trim(new [] {'\'', '"'}).ToLower();

				if (stopWords.Contains(loweredTokenText))
					continue;

				string stem = stemmer.Stem(loweredTokenText);
				
				if (!StemmedWords.ContainsKey(loweredTokenText))
					StemmedWords.Add(loweredTokenText, stem);
				Words.Add(stem);
				if (WordsFrequency.ContainsKey(stem))
					WordsFrequency[stem].Count++;
				else
					WordsFrequency.Add(stem, new WordFrequency() { Count = 1, Frequency = 0 } );
			}

			foreach (var frequency in WordsFrequency)
			{
				frequency.Value.Frequency = (double)frequency.Value.Count / Words.Count;
			}

			ExtractLabels();

			DocumentsCount++;
		}

		public List<DocumentRating> CalculateRatings(List<BagOfWords> bags)
		{
			Clustering clustering = new Clustering();
			var ratings = new List<DocumentRating>();
			for (int i = 0; i < bags.Count - 1; i++)
			{
				BagOfWords bag = bags[i];
				if (Interest != null && Interest.LikedDocs.Contains(bag.DocumentID))
					continue;
				double sim = clustering.GetCosineDistance(this, bag, true);
				ratings.Add(new DocumentRating() { ID = bag.DocumentID, Name = bag.Name, Type = bag.Type, Rating = sim });
			}

			return ratings.OrderByDescending(rating => rating.Rating).ToList();
		}

		public List<KeyValuePair<string, WordFrequency>> GetSortedWords()
		{
			return GetSortedDictionary(WordsFrequency);
		}

		public List<KeyValuePair<string, WordFrequency>> GetSortedLabels()
		{
			return GetSortedDictionary(Labels);
		}

		private List<KeyValuePair<string, WordFrequency>> GetSortedDictionary(Dictionary<string, WordFrequency> dict)
		{
			return dict.OrderByDescending(value => value.Value.Count).ToList();
		}

		private string NormalizeText(string text)
		{
			return text.Replace("‘", "'").Replace("’", "'").Replace("“", "\"").Replace("”", "\"");
		}

		private Tokenizer InitializeTokenizer()
		{
			Tokenizer tokenizer = new Tokenizer();
			tokenizer.WhitespaceChars = "";
			tokenizer.WhitespaceChars += "  ";		// Spaces
			tokenizer.WhitespaceChars += "\t";		// Tab
			tokenizer.WhitespaceChars += "\r";		// Carriage-Return
			tokenizer.WhitespaceChars += "\n";		// Linefeed/Newline

			tokenizer.SymbolChars = "";
			tokenizer.SymbolChars += ".,:;?!\"…";		// Continuation characters.
			tokenizer.SymbolChars += "+-*/^–";		// Mathematical operators.
			tokenizer.SymbolChars += "=<>";			// Conditional operators.
			tokenizer.SymbolChars += "()[]{}«»";		// Grouping chracters.

			tokenizer.LiteralDelimiters = "";
			//tokenizer.LiteralDelimiters += "'";		// Single quote.
			//tokenizer.LiteralDelimiters += "\"";	// Double quote.

			//tokenizer.LiteralEscapeChar = "\\";		// A C-like '\' escape character.

			return tokenizer;
		}

		private void ReadStopWords()
		{
			Assembly assembly = Assembly.GetExecutingAssembly();
			var a = assembly.GetManifestResourceNames();
			using (StreamReader textStreamReader = new StreamReader(assembly.GetManifestResourceStream("ReadReco.Model.resources.stopwords.stopwords.en")))
			{
				while (!textStreamReader.EndOfStream)
				{
					stopWords.Add(textStreamReader.ReadLine().ToLower());
				}
			}
		}

		private void ExtractLabels()
		{
			Labels.Clear();

			var sortedFreq = GetSortedWords().Where(wf => wf.Value.Count > 1).Reverse();
			double labelThreshold = WordsFrequency.Count * LabelThreshold;

			int current = WordsFrequency.Count + 1;
			var selectedFreq = new List<Tuple<string,WordFrequency>>();
			foreach (var freq in sortedFreq)
			{
				if (freq.Value.Count < current)
				{
					if (selectedFreq.Count + Labels.Count > labelThreshold)
						break;

					foreach (var sel in selectedFreq)
						Labels.Add(sel.Item1, sel.Item2);
					
					selectedFreq = new List<Tuple<string,WordFrequency>>();
					current = freq.Value.Count;
				}
				selectedFreq.Add(new Tuple<string, WordFrequency>(freq.Key, freq.Value));
			}

			if (selectedFreq.Count > 0 && Labels.Count == 0)
			{
				foreach (var sel in selectedFreq)
					Labels.Add(sel.Item1, sel.Item2);
			}

			//Labels = WordsFrequency.Where(wf => wf.Value.Frequency > LabelThreshold).ToDictionary(t => t.Key, t => t.Value);
		}

		public class WordFrequency
		{
			public int Count { get; set; }
			public double Frequency { get; set; }
		}
	}
}
