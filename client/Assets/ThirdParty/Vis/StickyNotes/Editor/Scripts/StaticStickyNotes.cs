using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using VIS.StickyNotes.ScriptableObjects;
using Object = UnityEngine.Object;

namespace VIS.StickyNotes.Editor
{
    public static class StaticStickyNotes
    {
        private const string _createMenuPath = "Assets/Vis/StickyNotes/Create";
        private const string _addMenuPath = "Assets/Vis/StickyNotes/Add Sticky Note";
        private const string _removeMenuPath = "Assets/Vis/StickyNotes/Remove Sticky Note";

        [MenuItem(_createMenuPath, false)]
        public static void CreateNew()
        {
            var path = default(string);
            var obj = Selection.activeObject;
            if (obj == null)
                path = "Assets";
            else
                path = AssetDatabase.GetAssetPath(obj.GetInstanceID());
            var newNote = ScriptableObject.CreateInstance<StickyNote>();
            ProjectWindowUtil.CreateAsset(newNote, Path.Combine(path, "Note.asset"));
            AssetDatabase.SaveAssets();
        }

        [MenuItem(_createMenuPath, true)]
        public static bool CreateNewValidation()
        {
            return Selection.activeObject != null && Selection.activeObject is DefaultAsset;
        }

        [MenuItem(_addMenuPath, false)]
        public static void AddStickyNoteToAsset()
        {
            var targetAsset = getAddableAsset();
            var newStickyAsset = ScriptableObject.CreateInstance<StickyNote>();
            newStickyAsset.name = "Note";
            AssetDatabase.AddObjectToAsset(newStickyAsset, targetAsset);
            EditorUtility.SetDirty(targetAsset);
            AssetDatabase.SaveAssets();

            notifyOnStickNotes();
        }

        [MenuItem(_addMenuPath, true)]
        public static bool AddableAssetValidation()
        {
            return getAddableAsset() != null;
        }

        //[MenuItem(_createMenuPath, false)]
        //public static void CreateNewStickyNote()
        //{
        //    var newStickyAsset = ScriptableObject.CreateInstance<StickyNote>();
        //    newStickyAsset.name = "Note";
        //    AssetDatabase.Sav
        //}

        //[MenuItem(_createMenuPath, true)]
        //public static bool CreateNewStickyNoteValidation()
        //{
        //    return Selection.objects == null || Selection.objects.Length == 0;
        //}

        [MenuItem(_removeMenuPath, false)]
        public static void RemoveStickyNoteFromAsset()
        {
            var targetAsset = getRemovableAsset();
            var mainAsset = (Object)null;
            if (AssetDatabase.IsSubAsset(targetAsset))
                mainAsset = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(targetAsset));
            var path = AssetDatabase.GetAssetPath(targetAsset);
            Object.DestroyImmediate(targetAsset, true);
            AssetDatabase.ImportAsset(path);
            //AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(targetAsset));
            //AssetDatabase.RemoveObjectFromAsset(targetAsset);
            if (mainAsset != null)
                EditorUtility.SetDirty(mainAsset);
            else
                AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            //Object.DestroyImmediate(targetAsset);

            notifyOnUnstickNotes();
        }

        [MenuItem(_removeMenuPath, true)]
        public static bool RemovableAssetValidation()
        {
            return getRemovableAsset() != null;
        }

        private static Object getAddableAsset()
        {
            var selected = Selection.objects;
            if (selected == null || selected.Length > 1)
                return null;

            var candidate = selected[0];
#if StickyDebug
            Debug.Log($"candidate = {candidate.GetType().FullName}");
#endif

            if (candidate is DefaultAsset || candidate is StickyNote || candidate is MonoScript || candidate is SceneAsset ||
                candidate is Shader || candidate is AssemblyDefinitionAsset)
                return null;

            if (!AssetDatabase.IsMainAsset(candidate) && !AssetDatabase.IsSubAsset(candidate))
                return null;

            return candidate;
        }

        private static Object getRemovableAsset()
        {
            var selected = Selection.objects;

            if (selected == null || selected.Length > 1)
                return null;

            if (!(selected[0] is StickyNote))
                return null;

            return selected[0];
        }

        private static void notifyOnStickNotes()
        {
            var allEditors = Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();
            var listeners = allEditors.Where(o => o is IAssetsStickedEventsListener).Select(o => o as IAssetsStickedEventsListener).ToArray();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i].OnSticked();
        }

        private static void notifyOnUnstickNotes()
        {
            var allEditors = Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();
            var listeners = allEditors.Where(o => o is IAssetsStickedEventsListener).Select(o => o as IAssetsStickedEventsListener).ToArray();
            for (int i = 0; i < listeners.Length; i++)
                listeners[i].OnUnsticked();
        }
    }
}
