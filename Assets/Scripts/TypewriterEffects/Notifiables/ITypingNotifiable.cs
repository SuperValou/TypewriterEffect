namespace Assets.Scripts.TypewriterEffects.Notifiables
{
    public interface ITypingNotifiable
    {
        void OnTypingBegin();
        void OnCaretMove();
        void OnTypingEnd();
    }
}