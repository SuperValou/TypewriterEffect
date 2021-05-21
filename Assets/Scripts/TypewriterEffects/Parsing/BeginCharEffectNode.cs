using Assets.Scripts.TypewriterEffects.Anims;

namespace Assets.Scripts.TypewriterEffects.Parsing
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