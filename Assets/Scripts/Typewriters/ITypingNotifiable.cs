namespace Assets.Scripts.Typewriters
{
    public interface ITypingNotifiable
    {
        void OnTypingBegin();
        void OnCaretMove();
        void OnTypingEnd();
    }
}