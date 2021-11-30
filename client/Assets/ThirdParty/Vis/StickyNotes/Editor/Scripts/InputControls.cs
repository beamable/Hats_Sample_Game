using UnityEditor;
using UnityEngine;

namespace VIS.StickyNotes.Editor
{
    public static class InputControls
    {
        public static KeyCode KeyCodeField(Rect controlRect, KeyCode keyCode)
        {
            var controlID = GUIUtility.GetControlID(FocusType.Keyboard);

            var retVal = keyCode;

            var evt = Event.current;

            switch (evt.GetTypeForControl(controlID))
            {
                case EventType.Repaint:
                    {
                        var style = GUI.skin.GetStyle("TextField");
                        if (style == GUIStyle.none) Debug.Log("GUI Style not found");
                        style.Draw(controlRect, new GUIContent(keyCode.ToString()), controlID);
                        break;
                    }
                case EventType.MouseDown:
                    {
                        if (controlRect.Contains(Event.current.mousePosition) && Event.current.button == 0 && GUIUtility.hotControl == 0)
                        {
                            Debug.Log("mouse down event, control id: " + controlID + ", hot control: " + GUIUtility.hotControl);
                            GUIUtility.hotControl = controlID;
                            GUIUtility.keyboardControl = controlID;
                            evt.Use();
                        }
                        break;
                    }
                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == controlID)
                        {
                            GUIUtility.hotControl = 0;
                            evt.Use();
                        }
                        break;

                    }
                case EventType.KeyDown:
                    {
                        Debug.Log("key down, control id: " + controlID + ", hot control: " + GUIUtility.hotControl);
                        if (GUIUtility.keyboardControl == controlID)
                        {
                            Debug.Log("hotcontrol");
                            retVal = Event.current.keyCode;
                            GUIUtility.hotControl = 0;
                            GUIUtility.keyboardControl = 0;
                            evt.Use();
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {

                        break;
                    }
            }
            return retVal;
        }

        public static KeyCode KeyCodeFieldLayout(KeyCode keyCode)
        {
            return KeyCodeField(EditorGUILayout.GetControlRect(), keyCode);
        }
    }
}