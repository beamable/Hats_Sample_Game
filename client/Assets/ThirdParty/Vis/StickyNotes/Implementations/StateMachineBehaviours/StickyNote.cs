using System;
using UnityEngine;

namespace VIS.StickyNotes.StateMachineBehaviours
{
    public class StickyNote : StateMachineBehaviour, IStickyNote
    {
#pragma warning disable
        public event Action<StickyNote, string> ConsoleTextEntered;

        [SerializeField, HideInInspector]
        private string _headerText = "Description";
        [SerializeField, HideInInspector]
        private string _text = "This is State!";
        [SerializeField, HideInInspector]
        private Color _color = Color.yellow;

        [SerializeField, HideInInspector]
        private string _consoleText;
#pragma warning enable


        [SerializeField, HideInInspector]
        private StickyNoteMode _mode = StickyNoteMode.Default;

        /// <summary>
        /// Write text
        /// </summary>
        public void Write(string text)
        {
#if UNITY_EDITOR
            _text = string.Format("{0}{1}", _text, text);
#endif
        }

        /// <summary>
        /// Write text and add End of Line symbols at the end
        /// </summary>
        public void WriteLine(string line)
        {
#if UNITY_EDITOR
            _text = string.Format("{0}{1}{2}", _text, line, "\r\n");
#endif
        }

        /// <summary>
        /// WriteLine with current date and time at the beginning
        /// </summary>
        public void Log(string line)
        {
#if UNITY_EDITOR
            WriteLine(string.Format("{0}. {1}", DateTime.Now, line));
#endif
        }

        /// <summary>
        /// Clear the text
        /// </summary>
        public void ClearBody()
        {
#if UNITY_EDITOR
            _text = default(string);
#endif
        }

#if UNITY_EDITOR
        public void TriggerConsoleTextEntered(string text)
        {
            if (ConsoleTextEntered != null)
                ConsoleTextEntered.Invoke(this, text);
        }
#endif

        [ContextMenu("Console mode")]
        private void consoleMode()
        {
            _mode = StickyNoteMode.Console;
        }
        [ContextMenu("Defualt mode")]
        private void defaultMode()
        {
            _mode = StickyNoteMode.Default;
        }
    }
}
