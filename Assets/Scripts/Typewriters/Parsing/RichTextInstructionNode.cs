namespace Assets.Scripts.Typewriters.Parsing
{
    internal class RichTextInstructionNode : INode
    {
        public string Value { get; }

        public RichTextInstructionNode(string value)
        {
            Value = value;
        }
    }
}