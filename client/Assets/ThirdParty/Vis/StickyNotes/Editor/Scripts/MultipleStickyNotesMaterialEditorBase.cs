using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VIS.StickyNotes.ScriptableObjects;
using Object = UnityEngine.Object;

namespace VIS.StickyNotes.Editor
{
    public abstract class MultipleStickyNotesMaterialEditorBase : MaterialEditor, IAssetsStickedEventsListener
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
                        () => _targetsCache.Length,
                        getTarget,
                        Repaint,
                        getSerializedObject
                    );

                return _stickyNoteEditorBehaviourBackingField;
            }
        }
        private GenericStickyNoteEditorBehaviour _stickyNoteEditorBehaviourBackingField;

        private SerializedObject[] _targetsCache;

        public override void OnEnable()
        {
            base.OnEnable();

            if (_targetsCache == null)
                setRightTarget();

            //Debug.Log($"Material OnEnable. _targetCache = {_targetCache}");
            if (_targetsCache != null)
                _stickyNoteEditorBehaviour.OnEnable();
        }

        public override void OnDisable()
        {
            if (_targetsCache != null)
                _stickyNoteEditorBehaviour.OnDisable();

            base.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            //Debug.Log($"Material OnInspectorGUI. _targetCache = {_targetCache}");
            if (_targetsCache == null)
                base.OnInspectorGUI();
            else
                _stickyNoteEditorBehaviour.OnInspectorGUI();
        }

        private SerializedProperty findProperty(int index, string propertyName)
        {
            return _targetsCache[index].FindProperty(propertyName);
        }

        private void applyModifiedProperties(int index)
        {
            _targetsCache[index].ApplyModifiedProperties();
        }

        private bool needToDrawBaseInspector
        {
            get
            {
                return true;
            }
        }

        private void setRightTarget()
        {
            var assetPath = AssetDatabase.GetAssetPath(target);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(a => a is StickyNote).Select(a => a as StickyNote);
            if (assets != null && assets.Count() > 0)
                _targetsCache = assets.Select(a => new SerializedObject(a)).ToArray();
        }

        public void OnSticked()
        {
            _targetsCache = null;
            _stickyNoteEditorBehaviourBackingField = null;
            OnEnable();
        }

        public void OnUnsticked()
        {
            _targetsCache = null;
            _stickyNoteEditorBehaviourBackingField = null;
            OnEnable();
        }

        protected virtual bool needCloseButton(int index)
        {
            return true;
        }
        protected virtual Object getTarget(int index)
        {
            return null;
        }

        protected virtual Action<int> closeButtonCallback
        {
            get
            {
                return index =>
                {
                    var assetPath = AssetDatabase.GetAssetPath(target);
                    var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath).Where(a => a is StickyNote).Select(a => a as StickyNote).ToArray();
                    for (int i = 0; i < assets.Length; i++)
                    {
                        if (assets[i] == _targetsCache[index].targetObject)
                        {
                            var path = AssetDatabase.GetAssetPath(assets[i]);
                            DestroyImmediate(assets[i], true);
                            AssetDatabase.ImportAsset(path);
                            //AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(assets[i]));
                            //AssetDatabase.RemoveObjectFromAsset(assets[i]);
                            AssetDatabase.SaveAssets();
                            OnUnsticked();
                            break;
                        }
                    }
                };
            }
        }

        protected virtual Func<int, SerializedObject> getSerializedObject
        {
            get
            {
                return index => _targetsCache[index];
            }
        }
    }
}
