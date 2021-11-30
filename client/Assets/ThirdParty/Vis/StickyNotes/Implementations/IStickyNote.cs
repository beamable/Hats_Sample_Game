namespace VIS.StickyNotes
{
    public interface IStickyNote
    {
        void WriteLine(string line);
#if UNITY_EDITOR
        void TriggerConsoleTextEntered(string text);
#endif
    }
}
