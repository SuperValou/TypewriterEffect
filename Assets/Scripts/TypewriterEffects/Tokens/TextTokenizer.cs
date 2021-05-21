using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Assets.Scripts.TypewriterEffects.Tokens
{
    public class TextTokenizer
    {
        private static readonly Regex RawTextRegex = new Regex(@"(?<text>.*?)<", RegexOptions.Singleline);

        private static readonly Regex PauseRegex = new Regex(@"\G<pause:(?<pauseLength>\w+)>", RegexOptions.None); // Matches "<pause:short>"
        private static readonly Regex SpeedRegex = new Regex(@"\G<speed:(?<speedValue>\w+)>", RegexOptions.None); // Matches "<speed:4>"
        private static readonly Regex AnimStartRegex = new Regex(@"\G<anim:(?<animType>\w+)>", RegexOptions.None); // Matches "<anim:wave>"
        private static readonly Regex AnimEndRegex = new Regex(@"\G<\/anim>", RegexOptions.None); // Matches "</anim>"
        private static readonly Regex RichTextRegex = new Regex(@"\G(?:<b>|<\/b>|<i>|<\/i>|<size=.+?>|<\/size>|<color=.+?>|<\/color>|<material=.+?>|<\/material>|<quad material=.+?>)", RegexOptions.None); // Matches stuff like "<size=50%>", see: https://docs.unity3d.com/Packages/com.unity.ugui@1.0/manual/StyledText.html
        private static readonly Regex SpriteRegex = new Regex(@"\G<sprite=.+?>", RegexOptions.None); // Matches <sprite="assetName" name="spriteName">

        private int _currentIndex;
        private TokenizerState _state;

        private List<Token> _tokens;
        
        public void Tokenize(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            _tokens = new List<Token>();
            _currentIndex = 0;

            while (_currentIndex < text.Length)
            {
                switch (_state)
                {
                    case TokenizerState.ReadingText:
                        var textMatch = RawTextRegex.Match(text, _currentIndex);
                        if (textMatch.Success)
                        {
                            string rawText = textMatch.Groups["text"].ToString();
                            if (rawText != string.Empty)
                            {
                                var rawTextToken = new Token(TokenType.RawText, rawText);
                                _tokens.Add(rawTextToken);
                            }

                            _currentIndex += rawText.Length;
                            _state = TokenizerState.ReadingTag;
                        }
                        else
                        {
                            var rawTextToken = new Token(TokenType.RawText, text.Substring(_currentIndex));
                            _tokens.Add(rawTextToken);
                            _currentIndex = text.Length;
                            return;
                        }
                        break;

                    case TokenizerState.ReadingTag:
                        // Check for pause tag
                        var match = PauseRegex.Match(text, _currentIndex);
                        if (match.Success)
                        {
                            string pauseLength = match.Groups["pauseLength"].ToString();

                            _tokens.Add(new Token(TokenType.OpeningTag, "<"));
                            _currentIndex += "<".Length;

                            _tokens.Add(new Token(TokenType.PauseTag, "pause:"));
                            _currentIndex += "pause:".Length;

                            _tokens.Add(new Token(TokenType.Value, pauseLength));
                            _currentIndex += pauseLength.Length;

                            _tokens.Add(new Token(TokenType.ClosingTag, ">"));
                            _currentIndex += ">".Length;
                            
                            _state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for speed tag
                        match = SpeedRegex.Match(text, _currentIndex);
                        if (match.Success)
                        {
                            string speedValue = match.Groups["speedValue"].ToString();

                            _tokens.Add(new Token(TokenType.OpeningTag, "<"));
                            _currentIndex += "<".Length;

                            _tokens.Add(new Token(TokenType.SpeedChangeTag, "speed:"));
                            _currentIndex += "speed:".Length;

                            _tokens.Add(new Token(TokenType.Value, speedValue));
                            _currentIndex += speedValue.Length;

                            _tokens.Add(new Token(TokenType.ClosingTag, ">"));
                            _currentIndex += ">".Length;

                            _state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for anim start tag
                        match = AnimStartRegex.Match(text, _currentIndex);
                        if (match.Success)
                        {
                            string animType = match.Groups["animType"].ToString();

                            _tokens.Add(new Token(TokenType.OpeningTag, "<"));
                            _currentIndex += "<".Length;

                            _tokens.Add(new Token(TokenType.AnimStartTag, "anim:"));
                            _currentIndex += "anim:".Length;

                            _tokens.Add(new Token(TokenType.Value, animType));
                            _currentIndex += animType.Length;

                            _tokens.Add(new Token(TokenType.ClosingTag, ">"));
                            _currentIndex += ">".Length;

                            _state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for anim end tag
                        match = AnimEndRegex.Match(text, _currentIndex);
                        if (match.Success)
                        {
                            _tokens.Add(new Token(TokenType.AnimEndTag, match.Value));
                            _currentIndex += match.Value.Length;

                            _state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for sprite tag
                        match = SpriteRegex.Match(text, _currentIndex);
                        if (match.Success)
                        {
                            _tokens.Add(new Token(TokenType.SpriteInstruction, match.Value));
                            _currentIndex += match.Value.Length;

                            _state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for rich text tag
                        match = RichTextRegex.Match(text, _currentIndex);
                        if (match.Success)
                        {
                            _tokens.Add(new Token(TokenType.RichTextInstruction, match.Value));
                            _currentIndex += match.Value.Length;

                            _state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Ignore other tags
                        _tokens.Add(new Token(TokenType.RawText, "<"));
                        _currentIndex += "<".Length;
                        _state = TokenizerState.ReadingText;
                        break;
                }
            }
        }

        public ICollection<Token> GetTokens()
        {
            if (_tokens == null)
            {
                throw new InvalidOperationException($"No text was tokenized. Did you forget to call the {nameof(Tokenize)} method?");
            }

            return _tokens.ToList();
        }
    }
}