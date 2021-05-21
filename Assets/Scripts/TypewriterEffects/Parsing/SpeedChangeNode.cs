namespace Assets.Scripts.TypewriterEffects.Parsing
{
    internal class SpeedChangeNode : INode
    {
        public float Speed { get; }

        public SpeedChangeNode(float speed)
        {
            Speed = speed;
        }
    }
}