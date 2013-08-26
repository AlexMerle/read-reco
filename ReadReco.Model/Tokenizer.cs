using System.Collections.Generic;

public class Tokenizer
{
	//---------------------------------------------------------------------
	public enum TokenType
	{
		Unknown = 0,
		Whitespace,
		Symbol,
		Literal,
		Identifier,
		Numeric,
		Keyword
	}

	//---------------------------------------------------------------------
	public class TokenList : List<Token> { }
	public class Token
	{
		public TokenType Type { get; set; }
		public string Text { get; set; }
		public int StartPosition { get; set; }
	}

	//---------------------------------------------------------------------
	public string WhitespaceChars { get; set; }
	public string SymbolChars { get; set; }
	public string LiteralDelimiters { get; set; }
	public string LiteralEscapeChar { get; set; }
	public List<string> Keywords { get; set; }
	public bool KeepWhitespace { get; set; }

	//---------------------------------------------------------------------
	public Tokenizer()
	{
		this.WhitespaceChars = "";
		this.SymbolChars = "";
		this.LiteralDelimiters = "";
		this.LiteralEscapeChar = "";
		this.Keywords = new List<string>();
		this.KeepWhitespace = false;
		return;
	}

	//---------------------------------------------------------------------
	public TokenList Tokenize( string Text )
	{
		using( System.IO.StringReader reader = new System.IO.StringReader( Text ) )
		{
			return this.Tokenize( reader );
		}
	}

	//---------------------------------------------------------------------
	public TokenList Tokenize( System.IO.TextReader Text )
	{
		TokenList tokens = new TokenList();
		Token token = new Token();
		int nChar = 0;

		while( true )
		{
			int CharCode = Text.Peek();
			if( CharCode == -1 ) { break; }

			if( this.WhitespaceChars.Contains( "" + (char)CharCode ) )
			{
				// Accumulate a Whitespace character.
				if( token.Type != TokenType.Whitespace )
				{
					this.CollectToken_( tokens, token );
					token = this.NewToken_( TokenType.Whitespace, "", nChar );
				}
				token.Text += (char)Text.Read();
				nChar++;
			}
			else if( this.SymbolChars.Contains( "" + (char)CharCode ) )
			{
				// Accumulate a single Symbol character.
				this.CollectToken_( tokens, token );
				token = this.NewToken_( TokenType.Symbol, ("" + (char)Text.Read()), nChar );
				this.CollectToken_( tokens, token );
				nChar++;
				token = this.NewToken_( TokenType.Unknown, "", nChar );
			}
			else if( this.LiteralDelimiters.Contains( "" + (char)CharCode ) )
			{
				// Accumulate a Literal character.
				this.CollectToken_( tokens, token );
				token = this.NewToken_( TokenType.Literal, ("" + (char)Text.Read()), nChar );
				nChar++;
				// Read until the closing delimiter.
				while( true )
				{
					int NextCharCode = Text.Peek();
					if( NextCharCode == -1 )
					{
						this.CollectToken_( tokens, token );
						token = this.NewToken_( TokenType.Unknown, "", nChar );
						break;
					}
					else if( NextCharCode == CharCode )
					{
						token.Text += (char)Text.Read();
						nChar++;
						if( !token.Text.EndsWith( this.LiteralEscapeChar + (char)CharCode ) )
						{
							this.CollectToken_( tokens, token );
							token = this.NewToken_( TokenType.Unknown, "", nChar );
							break;
						}
					}
					else
					{
						token.Text += (char)Text.Read();
						nChar++;
					}
				}
			}
			else
			{
				// Accumulate an identifier.
				if( token.Type != TokenType.Identifier )
				{
					this.CollectToken_( tokens, token );
					token = this.NewToken_( TokenType.Identifier, "", nChar );
				}
				token.Text += (char)Text.Read();
				nChar++;
			}

		}
		this.CollectToken_( tokens, token );

		// Return the tokens.
		return tokens;
	}


	//---------------------------------------------------------------------
	private void CollectToken_( TokenList Tokens, Token CurrentToken )
	{
		// Do some post-processing on the token and add to list of tokens.
		if( CurrentToken != null )
		{
			switch( CurrentToken.Type )
			{
				case TokenType.Unknown:
					// Do nothing, don't add to list.
					break;

				case TokenType.Whitespace:
					// No post-processing, add to list.
					if( this.KeepWhitespace )
					{
						Tokens.Add( CurrentToken );
					}
					break;

				case TokenType.Symbol:
					// No post-processing, add to list.
					Tokens.Add( CurrentToken );
					break;

				case TokenType.Literal:
					// No post-processing, add to list.
					Tokens.Add( CurrentToken );
					break;

				case TokenType.Identifier:

					// Check for Numeric.
					if( CurrentToken.Type == TokenType.Identifier )
					{
						double dValue = 0.0;
						if( double.TryParse( CurrentToken.Text, out dValue ) )
						{
							CurrentToken.Type = TokenType.Numeric;
						}
					}

					// Check for Keyword.
					if( CurrentToken.Type == TokenType.Identifier )
					{
						foreach( string keyword in this.Keywords )
						{
							if( string.Equals( CurrentToken.Text, keyword, System.StringComparison.InvariantCultureIgnoreCase ) )
							{
								CurrentToken.Type = TokenType.Keyword;
								break;
							}
						}
					}

					// Add to list.
					Tokens.Add( CurrentToken );
					break;
			}
		}

		return;
	}


	//---------------------------------------------------------------------
	private Token NewToken_( TokenType NewType, string NewText, int NewStartPosition )
	{
		// Return a new token.
		Token NewToken = new Token();
		NewToken.Type = NewType;
		NewToken.Text = NewText;
		NewToken.StartPosition = NewStartPosition;
		return NewToken;
	}


}
