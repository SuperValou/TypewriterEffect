using System;

namespace Assets.Scripts.TypewriterEffects.Tokens
{
    public class Token
    {
        public TokenType Type { get; }
        public string Value { get; }
        
        public Token(TokenType type, string value)
        {
            Type = type;
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public override string ToString()
        {
            return $"{Type}'{Value}'";
        }
    }
}