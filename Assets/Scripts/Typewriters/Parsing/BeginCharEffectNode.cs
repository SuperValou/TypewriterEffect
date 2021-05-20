using Assets.Scripts.Typewriters.Anims;

namespace Assets.Scripts.Typewriters.Parsing
{
    internal class BeginCharEffectNode : INode
    {
        public CharEffect CharEffect { get; }

        public BeginCharEffectNode(CharEffect charEffect)
        {
            CharEffect = charEffect;
        }
    }
}