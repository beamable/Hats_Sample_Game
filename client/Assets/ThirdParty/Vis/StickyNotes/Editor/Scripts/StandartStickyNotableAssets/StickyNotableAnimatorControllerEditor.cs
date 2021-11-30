using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace VIS.StickyNotes.Editor.StandartStickyNotableAssets
{
    [CustomEditor(typeof(AnimatorController))]
    public class StickyNotableAnimatorControllerEditor : MultipleStickyNotesEditorBase
    {
        protected override Object getTarget(int index)
        {
            if (_targetsCache == null || _targetsCache.Length == 0)
                return null;
            return _targetsCache.Length > index ? _targetsCache[index].targetObject : null;
        }
    }
}
