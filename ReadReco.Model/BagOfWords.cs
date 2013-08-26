using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveonik.Stemmers;
using ReadReco.IO.Feeds;
using System.Reflection;
using System.IO;

namespace ReadReco.Model
{
	public class BagOfWords
	{
		public string Name { get; private set; }
		public string Type { get; private set; }

		public List<string> Words { get; private set; }
		public Dictionary<string, string> StemmedWords { get; private set; }
		public Dictionary<string, WordFrequency> WordsFrequency { get; private set; }
		public Dictionary<string, WordFrequency> Labels { get; private set; }
		public int DocumentsCount { get; private set; }

		private HashSet<string> stopWords = new HashSet<string>();
		private Tokenizer tokenizer;
		private const double LabelThreshold = 0.003;
		
		public BagOfWords()
		{
			Words = new List<string>();
			StemmedWords = new Dictionary<string, string>();
			WordsFrequency = new Dictionary<string, WordFrequency>();
			Labels = new Dictionary<string, WordFrequency>();

			tokenizer = InitializeTokenizer();
			ReadStopWords();
		}

		public BagOfWords(string name, string type) : this()
		{
			Name = name;
			Type = type;
		}

		public void AddDocument(string document)
		{
			Tokenizer.TokenList words = tokenizer.Tokenize(NormalizeText(document));

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

			FillLabels();

			DocumentsCount++;
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
			List<KeyValuePair<string, WordFrequency>> sortedList = dict.ToList();

			sortedList.Sort((firstPair, nextPair) =>
				{
					return firstPair.Value.Count.CompareTo(nextPair.Value.Count);
				}
			);

			return sortedList;
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
			tokenizer.SymbolChars += "()[]{}";		// Grouping chracters.

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

		private void FillLabels()
		{
			Labels = WordsFrequency.Where(wf => wf.Value.Frequency > LabelThreshold).ToDictionary(t => t.Key, t => t.Value);
		}

		public class WordFrequency
		{
			public int Count { get; set; }
			public double Frequency { get; set; }
		}
	}
}
