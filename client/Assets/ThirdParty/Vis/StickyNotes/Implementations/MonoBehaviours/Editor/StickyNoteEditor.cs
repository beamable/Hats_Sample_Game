using System;
using UnityEditor;
using VIS.StickyNotes.Editor;
using Object = UnityEngine.Object;

namespace VIS.StickyNotes.MonoBehaviours.Editor
{
    [CustomEditor(typeof(StickyNote))]
    public sealed class StickyNoteEditor : StickyNoteEditorBase
    {
        protected override Object getTarget(int index)
        {
            return target;
        }
        protected override Func<int, SerializedObject> getSerializedObject
        {
            get
            {
                return _ => serializedObject;
            }
        }
    }
}
