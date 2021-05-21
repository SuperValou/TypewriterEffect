namespace Assets.Scripts.TypewriterEffects.Anims
{
    public class CaretEffect
    {
        public float Delay { get; }
        public float Speed { get; }

        public CaretEffect(float delay, float speed)
        {
            Delay = delay;
            Speed = speed;
        }
    }
}