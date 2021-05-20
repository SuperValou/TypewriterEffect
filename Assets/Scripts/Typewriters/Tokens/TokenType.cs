namespace Assets.Scripts.Typewriters.Tokens
{
    public enum TokenType
    {
        RawText,
        RichTextInstruction,
        SpriteInstruction,
        OpeningTag,
        Value,
        ClosingTag,
        PauseTag,
        SpeedChangeTag,
        AnimStartTag,
        AnimEndTag
    }
}