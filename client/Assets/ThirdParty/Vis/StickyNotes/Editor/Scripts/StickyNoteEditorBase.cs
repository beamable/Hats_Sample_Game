using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VIS.StickyNotes.Editor
{
    public abstract class StickyNoteEditorBase : UnityEditor.Editor
    {
        private GenericStickyNoteEditorBehaviour _stickyNoteEditorBehaviour
        {
            get
            {
                if (_stickyNoteEditorBehaviourBackingField == null)
                    _stickyNoteEditorBehaviourBackingField = new GenericStickyNoteEditorBehaviour(
                        base.OnInspectorGUI,
                        findProperty,
                        applyModifiedProperties,
                        needCloseButton,
                        closeButtonCallback,
                        () => needToDrawBaseInspector,
                        () => 1,
                        getTarget,
                        Repaint,
                        getSerializedObject
                    );

                return _stickyNoteEditorBehaviourBackingField;
            }
        }
        private GenericStickyNoteEditorBehaviour _stickyNoteEditorBehaviourBackingField;

        private void OnEnable()
        {
            _stickyNoteEditorBehaviour.OnEnable();
        }

        private void OnDisable()
        {
            _stickyNoteEditorBehaviour.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            _stickyNoteEditorBehaviour.OnInspectorGUI();
        }

        protected virtual SerializedProperty findProperty(int index, string propertyName)
        {
            return serializedObject.FindProperty(propertyName);
        }
        protected virtual void applyModifiedProperties(int index)
        {
            serializedObject.ApplyModifiedProperties();
        }
        protected virtual bool needToDrawBaseInspector
        {
            get
            {
                return false;
            }
        }
        protected virtual bool needCloseButton(int index)
        {
            return false;
        }
        protected virtual Action<int> closeButtonCallback
        {
            get
            {
                return null;
            }
        }
        protected virtual Object getTarget(int index)
        {
            return null;
        }
        protected virtual Func<int, SerializedObject> getSerializedObject
        {
            get
            {
                return null;
            }
        }
    }
}
