namespace Assets.Scripts.Typewriters.Parsing
{
    internal class PauseNode : INode
    {
        public float PauseDuration { get; }

        public PauseNode(float pauseDuration)
        {
            PauseDuration = pauseDuration;
        }
    }
}