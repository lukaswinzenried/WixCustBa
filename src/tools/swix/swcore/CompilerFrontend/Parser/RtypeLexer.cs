﻿//-------------------------------------------------------------------------------------------------
// <copyright file="RtypeLexer.cs" company="Outercurve Foundation">
//   Copyright (c) 2004, Outercurve Foundation.
//   This software is released under Microsoft Reciprocal License (MS-RL).
//   The license and further copyright text can be found in the file
//   LICENSE.TXT at the root directory of the distribution.
// </copyright>
//-------------------------------------------------------------------------------------------------

namespace WixToolset.Simplified.CompilerFrontend.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using WixToolset.Simplified;
    using WixToolset.Simplified.ParserCore;

    public enum LexerTokenType
    {
        None,
        Unknown,
        Whitespace,
        Newline,
        Period,
        Equals,
        DoubleQuote,
        SingleQuote,
        Identifier,
        Number,
        UseKeyword,     // rtype only
        Hash,           // rtype only
        LeftAngle,      // xml only
        RightAngle,     // xml only
        Question,       // xml only
        Slash,          // xml only
        Exclamation,    // xml only
        DoubleDash,     // xml only
        Colon,          // xml only (actually, it is used in rtype, but for no reason!)
        Value,  // catch-all for sequences that aren't part of the above!
    }

    // TODO: move to ParserCore?
    public interface ITextProvider
    {
        bool TryGetText(Position pos, out string text, out Range range);
    }

    /// <summary>
    /// R-type lexer.
    /// </summary>
    public static class RtypeLexer
    {
        // ValueTokenEnders are characters that cause a Value token ("none of the above") to end,
        // so that some other kind of token can start.
        // TODO: make this a table to also use for token mapping?
        // TODO: make Value tokens only a single character long?
        private static readonly char[] ValueTokenEnders = new char[] { ' ', '\t', '\v', '\r', '\n', '#', '"', '\'' };
        private static char[] Newlines = new char[] { '\n', '\r' };

        public static IEnumerable<Token<LexerTokenType>> LexTokens(Position start, ITextProvider textProvider)
        {
            System.Diagnostics.Debug.Assert(textProvider != null);

            Position pos = start;
            string text;
            Range range;

            // So that the caller's implementation of 'tryGetMoreText' doesn't have to buffer and be
            // smart about token boundaries, we detect when we're about to yield the last token
            // in the current buffer, and instead try to get more text and re-lex.
            Token<LexerTokenType> lastToken = null;
            while (textProvider.TryGetText(pos, out text, out range))
            {
                if (lastToken != null)
                {
                    text = string.Concat(lastToken.Value, text);
                    range = new Range(lastToken.Range.Start, range.End);
                }

                foreach (Token<LexerTokenType> token in RtypeLexer.LexTokens(text, range.Start))
                {
                    if (token.Range.End.Offset != range.End.Offset)
                    {
                        lastToken = null;
                        yield return token;
                    }
                    else
                    {
                        lastToken = token;
                    }
                }

                pos = range.End;
            }

            // If we fell off the end with a token, make sure the caller gets it!
            if (lastToken != null)
            {
                yield return lastToken;
            }
        }

        public static IEnumerable<Token<LexerTokenType>> LexTokens(string text, Position startPosition)
        {
            Position tokenStart = startPosition;
            bool hadNewline = false;

            int currentPos = 0;
            while (currentPos < text.Length)
            {
                char currentChar = text[currentPos];
                var followingChars = text.Skip(currentPos + 1).TakeWhile(c => !Newlines.Contains(c));
                LexerTokenType tokenType = LexerTokenType.None;
                int length = 0;

                hadNewline = false;

                if (currentChar == '\r' || currentChar == '\n')
                {
                    tokenType = LexerTokenType.Newline;
                    // Take all consecutive newline characters, or just the immediate CRLF? ... just the next one.
                    length = (currentChar == '\r' && text.Skip(currentPos + 1).FirstOrDefault() == '\n') ? 2 : 1;
                    hadNewline = true;
                }
                else if (char.IsWhiteSpace(currentChar))
                {
                    // return all of the consecutive whitespace...
                    tokenType = LexerTokenType.Whitespace;
                    length = 1 + followingChars.TakeWhile(c => char.IsWhiteSpace(c)).Count();
                }
                else if (currentChar == 'u' &&
                    currentPos + 2 < text.Length &&
                    text[currentPos + 1] == 's' &&
                    text[currentPos + 2] == 'e' &&
                    (currentPos + 3 >= text.Length || char.IsWhiteSpace(text[currentPos + 3])))
                {
                    tokenType = LexerTokenType.UseKeyword;
                    length = 3;
                }
                else if (currentChar == '#')
                {
                    tokenType = LexerTokenType.Hash;
                    length = 1;
                }
                else if (currentChar == ':')
                {
                    tokenType = LexerTokenType.Colon;
                    length = 1;
                }
                else if (currentChar == '.')
                {
                    tokenType = LexerTokenType.Period;
                    length = 1;
                }
                else if (currentChar == '=')
                {
                    tokenType = LexerTokenType.Equals;
                    length = 1;
                }
                else if (currentChar == '"')
                {
                    tokenType = LexerTokenType.DoubleQuote;
                    length = 1;
                }
                else if (currentChar == '\'')
                {
                    tokenType = LexerTokenType.SingleQuote;
                    length = 1;
                }
                else if (char.IsLetter(currentChar))
                {
                    tokenType = LexerTokenType.Identifier;
                    length = 1 + followingChars.TakeWhile(c => char.IsLetterOrDigit(c) || c == '_').Count();
                }
                else if (char.IsDigit(currentChar))
                {
                    tokenType = LexerTokenType.Number;
                    length = 1 + followingChars.TakeWhile(c => char.IsDigit(c)).Count();
                }
                else
                {
                    tokenType = LexerTokenType.Value;
                    length = 1 + followingChars.TakeWhile(c => !ValueTokenEnders.Contains(c)).Count();
                }

                if (length <= 0)
                {
                    throw new Exception("didn't eat any characters!");
                }

                Range tokenRange = new Range(tokenStart, length);

                yield return new Token<LexerTokenType>(
                    tokenType,
                    text.Substring(currentPos, length),
                    tokenRange);

                currentPos += length;
                tokenStart = tokenRange.End;

                // After newlines, keep the offset, but bump the line and reset the column
                if (hadNewline)
                {
                    tokenStart = new Position(tokenStart.Offset, tokenStart.Line + 1, 0);
                }
            }
        }
    }
}
