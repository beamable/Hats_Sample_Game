using System;
using UnityEditor;
using VIS.StickyNotes.ScriptableObjects;

namespace VIS.StickyNotes.Editor
{
    public abstract class StickyNotableAssetEditorBase : StickyNoteEditorBase
    {
        private SerializedObject _targetCache;

        protected sealed override SerializedProperty findProperty(int index, string propertyName)
        {
            if (_targetCache == null)
                setRightTarget();
            return _targetCache.FindProperty(propertyName);
        }

        protected override void applyModifiedProperties(int index)
        {
            if (_targetCache == null)
                setRightTarget();
            _targetCache.ApplyModifiedProperties();
        }

        protected override bool needToDrawBaseInspector
        {
            get
            {
                return true;
            }
        }

        private void setRightTarget()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var asset = AssetDatabase.LoadAssetAtPath<StickyNote>(assetPath);
            _targetCache = new SerializedObject(asset);
        }
    }
}
