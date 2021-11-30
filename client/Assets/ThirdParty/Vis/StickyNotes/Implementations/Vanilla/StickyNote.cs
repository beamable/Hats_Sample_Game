using System;
using UnityEngine;

namespace VIS.StickyNotes.Vanilla
{
    [Serializable]
    public class StickyNote
    {
#pragma warning disable
        [SerializeField, HideInInspector]
        private string _headerText = "Description";
        [SerializeField, HideInInspector]
        private string _text = "This is State!";
        [SerializeField, HideInInspector]
        private Color _color = Color.yellow;
#pragma warning enable

        public StickyNote()
        {

        }

        public StickyNote(string headerText, string text, Color color)
        {
            _headerText = headerText;
            _text = text;
            _color = color;
        }
    }
}
