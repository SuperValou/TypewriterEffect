namespace Assets.Scripts.TypewriterEffects
{
    public interface ITypingNotifiable
    {
        void OnTypingBegin();
        void OnCaretMove();
        void OnTypingEnd();
    }
}