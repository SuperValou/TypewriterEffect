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
        
        public ICollection<Token> Tokenize(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            List<Token> tokens = new List<Token>();
            if (text == string.Empty)
            {
                return tokens;
            }
            
            int currentIndex = 0;
            TokenizerState state = TokenizerState.ReadingText;

            while (currentIndex < text.Length)
            {
                switch (state)
                {
                    case TokenizerState.ReadingText:
                        var textMatch = RawTextRegex.Match(text, currentIndex);
                        if (textMatch.Success)
                        {
                            string rawText = textMatch.Groups["text"].ToString();
                            if (rawText != string.Empty)
                            {
                                var rawTextToken = new Token(TokenType.RawText, rawText);
                                tokens.Add(rawTextToken);
                            }

                            currentIndex += rawText.Length;
                            state = TokenizerState.ReadingTag;
                        }
                        else
                        {
                            var rawTextToken = new Token(TokenType.RawText, text.Substring(currentIndex));
                            tokens.Add(rawTextToken);
                            return tokens;
                        }
                        break;

                    case TokenizerState.ReadingTag:
                        // Check for pause tag
                        var match = PauseRegex.Match(text, currentIndex);
                        if (match.Success)
                        {
                            string pauseLength = match.Groups["pauseLength"].ToString();

                            tokens.Add(new Token(TokenType.OpeningTag, "<"));
                            currentIndex += "<".Length;

                            tokens.Add(new Token(TokenType.PauseTag, "pause:"));
                            currentIndex += "pause:".Length;

                            tokens.Add(new Token(TokenType.Value, pauseLength));
                            currentIndex += pauseLength.Length;

                            tokens.Add(new Token(TokenType.ClosingTag, ">"));
                            currentIndex += ">".Length;
                            
                            state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for speed tag
                        match = SpeedRegex.Match(text, currentIndex);
                        if (match.Success)
                        {
                            string speedValue = match.Groups["speedValue"].ToString();

                            tokens.Add(new Token(TokenType.OpeningTag, "<"));
                            currentIndex += "<".Length;

                            tokens.Add(new Token(TokenType.SpeedChangeTag, "speed:"));
                            currentIndex += "speed:".Length;

                            tokens.Add(new Token(TokenType.Value, speedValue));
                            currentIndex += speedValue.Length;

                            tokens.Add(new Token(TokenType.ClosingTag, ">"));
                            currentIndex += ">".Length;

                            state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for anim start tag
                        match = AnimStartRegex.Match(text, currentIndex);
                        if (match.Success)
                        {
                            string animType = match.Groups["animType"].ToString();

                            tokens.Add(new Token(TokenType.OpeningTag, "<"));
                            currentIndex += "<".Length;

                            tokens.Add(new Token(TokenType.AnimStartTag, "anim:"));
                            currentIndex += "anim:".Length;

                            tokens.Add(new Token(TokenType.Value, animType));
                            currentIndex += animType.Length;

                            tokens.Add(new Token(TokenType.ClosingTag, ">"));
                            currentIndex += ">".Length;

                            state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for anim end tag
                        match = AnimEndRegex.Match(text, currentIndex);
                        if (match.Success)
                        {
                            tokens.Add(new Token(TokenType.AnimEndTag, match.Value));
                            currentIndex += match.Value.Length;

                            state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for sprite tag
                        match = SpriteRegex.Match(text, currentIndex);
                        if (match.Success)
                        {
                            tokens.Add(new Token(TokenType.SpriteInstruction, match.Value));
                            currentIndex += match.Value.Length;

                            state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Check for rich text tag
                        match = RichTextRegex.Match(text, currentIndex);
                        if (match.Success)
                        {
                            tokens.Add(new Token(TokenType.RichTextInstruction, match.Value));
                            currentIndex += match.Value.Length;

                            state = TokenizerState.ReadingText;
                            continue;
                        }

                        // Ignore other tags
                        tokens.Add(new Token(TokenType.RawText, "<"));
                        currentIndex += "<".Length;
                        state = TokenizerState.ReadingText;
                        break;
                }
            }

            return tokens;
        }
    }
}