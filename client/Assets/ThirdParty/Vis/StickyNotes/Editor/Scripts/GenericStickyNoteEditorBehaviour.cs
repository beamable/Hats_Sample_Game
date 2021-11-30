using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VIS.StickyNotes.Editor
{
    internal class GenericStickyNoteEditorBehaviour
    {
        private const float _bodyPadding = 8f;
        private const int _contentMargin = 6;
        private const float _headerHeight = 40f;
        private const float _closeButtonWidth = 29f;

        private SerializedProperty[] _descriptionPropsCache;
        private SerializedProperty[] _textPropsCache;
        private SerializedProperty[] _colorPropsCache;
        private SerializedProperty[] _modePropsCache;
        private SerializedProperty[] _consoleTextPropsCache;

        private StickyNoteState[] _states;

        private readonly GUIStyle _buttonStyles;
        private readonly GUIStyle _descriptionStyles;
        private readonly GUIStyle _labelStyles;
        private readonly GUIStyle _textAreaStyles;
        private readonly GUIStyle _topPanelStyle;
        private readonly GUIStyle _textFieldStyle;
        private readonly GUIStyle _oneLineLabelStyle;
        private readonly GUIStyle _closeButtonStyle;

        private Action _baseOnInspectorGUIAction;
        private Func<int, string, SerializedProperty> _findPropertyFunc;
        private Action<int> _applyModifiedPropertiesAction;
        private Func<int, bool> _needCloseButtonFunc;
        private Action<int> _closeButtonCallbacks;
        private Func<bool> _needToDrawBaseInspectorFunc;
        private Func<int> _notesCountFunc;
        private Func<int, Object> _getTargetFunc;
        private Action _repaintAction;
        private Func<int, SerializedObject> _getSerializedObject;

        private GUISkin _skin;
        private Texture _closeTexture;

        internal GenericStickyNoteEditorBehaviour(
            Action baseOnInspectorGUIAction,
            Func<int, string, SerializedProperty> findPropertyFunc,
            Action<int> applyModifiedPropertiesAction,
            Func<int, bool> needCloseButtonFunc,
            Action<int> closeButtonCallbacks,
            Func<bool> needToDrawBaseInspectorFunc,
            Func<int> notesCountFunc,
            Func<int, Object> getTargetFunc,
            Action repaintAction,
            Func<int, SerializedObject> getSerializedObject)
        {
            _baseOnInspectorGUIAction = baseOnInspectorGUIAction;
            _findPropertyFunc = findPropertyFunc;
            _applyModifiedPropertiesAction = applyModifiedPropertiesAction;
            _needToDrawBaseInspectorFunc = needToDrawBaseInspectorFunc;
            _notesCountFunc = notesCountFunc;
            _needCloseButtonFunc = needCloseButtonFunc;
            _closeButtonCallbacks = closeButtonCallbacks;
            _getTargetFunc = getTargetFunc;
            _repaintAction = repaintAction;
            _getSerializedObject = getSerializedObject;

            _skin = Resources.Load<GUISkin>("Vis/StickyNotes/StickyNoteGuiSkin");
            _closeTexture = Resources.Load<Texture>("Vis/StickyNotes/Textures/close-icon");

            _buttonStyles = _skin.GetStyle("Button");
            _descriptionStyles = _skin.GetStyle("Description");
            _labelStyles = _skin.GetStyle("Label");
            _textAreaStyles = _skin.GetStyle("TextArea");
            _topPanelStyle = _skin.GetStyle("TopPanel");
            _textFieldStyle = _skin.GetStyle("TextField");
            _oneLineLabelStyle = _skin.GetStyle("OneLineLabel");
            _closeButtonStyle = _skin.GetStyle("CloseButton");
        }

        internal void OnEnable()
        {
            if (_skin == null)
                return;
            var count = _notesCountFunc();

            _descriptionPropsCache = new SerializedProperty[count];
            _textPropsCache = new SerializedProperty[count];
            _colorPropsCache = new SerializedProperty[count];
            _modePropsCache = new SerializedProperty[count];
            _consoleTextPropsCache = new SerializedProperty[count];

            _states = new StickyNoteState[count];

            for (int i = 0; i < count; i++)
            {
                _descriptionPropsCache[i] = _findPropertyFunc(i, "_headerText");
                _textPropsCache[i] = _findPropertyFunc(i, "_text");
                _colorPropsCache[i] = _findPropertyFunc(i, "_color");
                _modePropsCache[i] = _findPropertyFunc(i, "_mode");
                _consoleTextPropsCache[i] = _findPropertyFunc(i, "_consoleText");
            }
        }

        internal void OnDisable()
        {
            _textPropsCache = null;
            _colorPropsCache = null;
            _descriptionPropsCache = null;
            _skin = null;
            _closeTexture = null;

            _states = null;
        }

        internal void OnInspectorGUI()
        {
            if (_needToDrawBaseInspectorFunc())
                _baseOnInspectorGUIAction();

            if (_skin == null)
                return;
            var count = _notesCountFunc();
            for (int i = 0; i < count; i++)
            {
                var borderRect = GUILayoutUtility.GetRect(new GUIContent(_textPropsCache[i].stringValue), _labelStyles/*, GUILayout.ExpandWidth(false)*/);
                borderRect.x = _bodyPadding;
                borderRect.width = EditorGUIUtility.currentViewWidth - _bodyPadding * 2f;
                var otherStuff = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - _bodyPadding * 2f, _contentMargin * 2f + _headerHeight);
                borderRect.height += otherStuff.size.y;

                var consoleRect = default(Rect);
                if (_modePropsCache[i].enumValueIndex == (int)StickyNoteMode.Console)
                {
                    consoleRect = GUILayoutUtility.GetRect(GUIContent.none, _textAreaStyles);
                    //borderRect.height += consoleRect.size.y;
                    consoleRect.size += Vector2.right * 8f;
                    consoleRect.position -= Vector2.right * 5f + Vector2.up * 8f;
                }

                var mainRect = borderRect;
                mainRect.x += 1;
                mainRect.y += 1 + _headerHeight;
                mainRect.width -= 2;
                mainRect.height -= 2 + _headerHeight;

                var headerRect = mainRect;
                headerRect.y -= _headerHeight;
                headerRect.height = _headerHeight;

                var textRect = mainRect;
                textRect.x += _contentMargin;
                textRect.y += _contentMargin;
                textRect.height -= _contentMargin * 2;
                textRect.width -= _contentMargin * 2;

                var e = Event.current;

                Handles.BeginGUI();
                switch (_states[i])
                {
                    case StickyNoteState.View:
                        {
                            Handles.DrawSolidRectangleWithOutline(borderRect, _colorPropsCache[i].colorValue, Color.white * 0.3f);
                            Handles.DrawSolidRectangleWithOutline(headerRect, _colorPropsCache[i].colorValue * 0.9f, _colorPropsCache[i].colorValue * 0.9f);

                            GUI.Label(textRect, _textPropsCache[i].stringValue, _labelStyles);

                            if (_modePropsCache[i].enumValueIndex == (int)StickyNoteMode.Console)
                            {
                                if (Event.current.isKey &&
                                    Event.current.type == EventType.KeyDown &&
                                    (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
                                {
                                    if (GUI.GetNameOfFocusedControl() == "Console")
                                        enterConsoleText(i);
                                }

                                Handles.DrawSolidRectangleWithOutline(consoleRect, _colorPropsCache[i].colorValue * 0.7f, _colorPropsCache[i].colorValue * 0.7f);
                                GUI.SetNextControlName("Console");
                                _consoleTextPropsCache[i].stringValue = EditorGUI.TextField(consoleRect, _consoleTextPropsCache[i].stringValue, _textAreaStyles);
                            }

                            GUILayout.BeginArea(headerRect);
                            GUILayout.BeginHorizontal(_topPanelStyle);
                            var descriptionRect = workaroundGetRect(new GUIContent(_descriptionPropsCache[i].stringValue), _descriptionStyles, headerRect, GUILayout.MaxWidth(200f));

                            GUILayout.FlexibleSpace();
                            var clearButtonContent = default(GUIContent);
                            var copyButtonContent = default(GUIContent);
                            var clearButtonRect = default(Rect?);
                            var copyButtonRect = default(Rect?);

                            if (_modePropsCache[i].enumValueIndex == (int)StickyNoteMode.Console)
                            {
                                clearButtonContent = new GUIContent("Clear");
                                clearButtonRect = workaroundGetRect(clearButtonContent, _buttonStyles, headerRect);

                                copyButtonContent = new GUIContent("Copy");
                                copyButtonRect = workaroundGetRect(copyButtonContent, _buttonStyles, headerRect);
                            }

                            var editButtonContent = new GUIContent("Edit");
                            Rect? editButtonRect = workaroundGetRect(editButtonContent, _buttonStyles, headerRect);

                            var closeButtonContent = default(GUIContent);
                            var closeButtonRect = default(Rect?);
                            if (_needCloseButtonFunc(i))
                            {
                                closeButtonContent = new GUIContent(_closeTexture, "Remove note");
                                closeButtonRect = workaroundGetRect(closeButtonContent, _closeButtonStyle, headerRect, GUILayout.Width(_closeButtonWidth), GUILayout.Height(_closeButtonWidth));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.EndArea();


                            GUI.Label(descriptionRect, _descriptionPropsCache[i].stringValue, _oneLineLabelStyle);
                            if (clearButtonRect.HasValue)
                            {
                                Handles.DrawSolidRectangleWithOutline(clearButtonRect.Value, _colorPropsCache[i].colorValue * 0.8f, _colorPropsCache[i].colorValue * 0.7f);
                                if (GUI.Button(clearButtonRect.Value, clearButtonContent, _buttonStyles))
                                    _textPropsCache[i].stringValue = string.Empty;
                            }
                            if (copyButtonRect.HasValue)
                            {
                                Handles.DrawSolidRectangleWithOutline(copyButtonRect.Value, _colorPropsCache[i].colorValue * 0.8f, _colorPropsCache[i].colorValue * 0.7f);
                                if (GUI.Button(copyButtonRect.Value, copyButtonContent, _buttonStyles))
                                    EditorGUIUtility.systemCopyBuffer = _textPropsCache[i].stringValue;
                            }
                            if (editButtonRect.HasValue)
                            {
                                Handles.DrawSolidRectangleWithOutline(editButtonRect.Value, _colorPropsCache[i].colorValue * 0.8f, _colorPropsCache[i].colorValue * 0.7f);
                                if (GUI.Button(editButtonRect.Value, editButtonContent, _buttonStyles))
                                    _states[i] = StickyNoteState.Edit;
                            }
                            if (closeButtonRect.HasValue)
                            {
                                Handles.DrawSolidRectangleWithOutline(closeButtonRect.Value, _colorPropsCache[i].colorValue * 0.8f, _colorPropsCache[i].colorValue * 0.7f);
                                if (GUI.Button(closeButtonRect.Value, closeButtonContent, _closeButtonStyle) && _closeButtonCallbacks != null)
                                    _closeButtonCallbacks.Invoke(i);
                            }

                            if (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command)
                            {
                                switch (e.keyCode)
                                {
                                    case KeyCode.E:
                                        _states[i] = StickyNoteState.Edit;
                                        _repaintAction();
                                        break;
                                }
                            }

                            if (_modePropsCache[i].enumValueIndex == (int)StickyNoteMode.Console)
                                _applyModifiedPropertiesAction(i);
                        }
                        break;
                    case StickyNoteState.Edit:
                        {
                            Handles.DrawSolidRectangleWithOutline(borderRect, Color.white * 0.3f, _colorPropsCache[i].colorValue);
                            Handles.DrawSolidRectangleWithOutline(headerRect, Color.white * 0.3f, _colorPropsCache[i].colorValue * 0.9f);

                            _textPropsCache[i].stringValue = EditorGUI.TextArea(textRect, _textPropsCache[i].stringValue, _textAreaStyles);

                            GUILayout.BeginArea(headerRect);
                            GUILayout.BeginHorizontal(_topPanelStyle);
                            var descriptionTextRect = workaroundGetRect(new GUIContent(_descriptionPropsCache[i].stringValue), _textFieldStyle, headerRect, GUILayout.MinWidth(200f));
                            GUILayout.FlexibleSpace();
                            var colorRect = workaroundGetRect(GUIContent.none, _skin.box, headerRect, GUILayout.MaxHeight(30f), GUILayout.MaxWidth(30f));
                            GUILayout.Space(4f);

                            var backButtonContent = new GUIContent("Back");
                            var backButtonRect = workaroundGetRect(backButtonContent, _buttonStyles, headerRect);

                            var closeButtonContent = default(GUIContent);
                            var closeButtonRect = default(Rect?);
                            if (_needCloseButtonFunc(i))
                            {
                                closeButtonContent = new GUIContent(_closeTexture, "Remove note");
                                closeButtonRect = workaroundGetRect(closeButtonContent, _closeButtonStyle, headerRect, GUILayout.Width(_closeButtonWidth), GUILayout.Height(_closeButtonWidth));
                            }
                            GUILayout.EndHorizontal();
                            GUILayout.EndArea();

                            _descriptionPropsCache[i].stringValue = EditorGUI.TextField(descriptionTextRect, _descriptionPropsCache[i].stringValue, _textFieldStyle);

#if UNITY_2017
                            _colorPropsCache[i].colorValue = EditorGUI.ColorField(colorRect, GUIContent.none, _colorPropsCache[i].colorValue, false, false, false, new ColorPickerHDRConfig(0, 1, 0, 1));
#else

                            _colorPropsCache[i].colorValue = EditorGUI.ColorField(colorRect, GUIContent.none, _colorPropsCache[i].colorValue, false, false, false);
#endif

                            Handles.DrawSolidRectangleWithOutline(backButtonRect, _colorPropsCache[i].colorValue * 0.8f, _colorPropsCache[i].colorValue * 0.7f);
                            if (GUI.Button(backButtonRect, backButtonContent, _buttonStyles))
                            {
                                _states[i] = StickyNoteState.View;
                                GUI.FocusControl(null);
                            }

                            if (closeButtonRect.HasValue)
                            {
                                Handles.DrawSolidRectangleWithOutline(closeButtonRect.Value, _colorPropsCache[i].colorValue * 0.8f, _colorPropsCache[i].colorValue * 0.7f);
                                if (GUI.Button(closeButtonRect.Value, closeButtonContent, _closeButtonStyle) && _closeButtonCallbacks != null)
                                    _closeButtonCallbacks.Invoke(i);
                            }

                            _applyModifiedPropertiesAction(i);
                            if (e.modifiers == EventModifiers.Control || e.modifiers == EventModifiers.Command)
                            {
                                switch (e.keyCode)
                                {
                                    case KeyCode.Return:
                                    case KeyCode.KeypadEnter:
                                        _states[i] = StickyNoteState.View;
                                        _repaintAction();
                                        break;
                                }
                            }
                        }
                        break;
                }
                Handles.EndGUI();

                var target = _getTargetFunc(i);
                if (target != null)
                {
                    if (target is Component)
                        UnityEditorInternal.ComponentUtility.MoveComponentDown(target as Component);
                }
            }
        }

        private Rect workaroundGetRect(GUIContent content, GUIStyle style, Rect headerRect, params GUILayoutOption[] options)
        {
            var result = GUILayoutUtility.GetRect(content, style, options);
            result.position += headerRect.position;
            return result;
        }

        private void enterConsoleText(int i)
        {
            (_getTargetFunc(i) as IStickyNote).WriteLine(_consoleTextPropsCache[i].stringValue);
            (_getTargetFunc(i) as IStickyNote).TriggerConsoleTextEntered(_consoleTextPropsCache[i].stringValue);
            _getSerializedObject(i).Update();
            _consoleTextPropsCache[i].stringValue = string.Empty;
        }
    }
}
