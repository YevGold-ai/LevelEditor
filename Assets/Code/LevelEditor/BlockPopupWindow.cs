#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Code.LevelEditor
{
    public class BlockPopupWindow : EditorWindow
    {
        public static BlockPopupWindow Current;

        private BlockLibrary _library;
        private List<Vector2Int> _currentSelection;

        private Action<BlockDataEditor> _onBlockSelected;
        private Action<float> _onRotationSelected;
        private Action _onClear;

        private Vector2 _scrollBlocks;
        private Vector2 _scrollSelection;

        private bool _isDragging = false;
        private Vector2 _dragOffset;
        
        private bool _showSelectionList = true;
        private string _searchFilter = "";
        
        private string _searchBlockFilter = "";
        
        public static void ShowPopup(
            Rect activatorRect,
            BlockLibrary library,
            Action<BlockDataEditor> onBlockSelected,
            Action<float> onRotationSelected,
            Action onClear,
            List<Vector2Int> currentSelection)
        {
            if (Current != null)
                Current.Close();
            
            Current = CreateInstance<BlockPopupWindow>();
            Current._library = library;
            Current._onBlockSelected = onBlockSelected;
            Current._onRotationSelected = onRotationSelected;
            Current._onClear = onClear;
            Current._currentSelection = currentSelection;
            
            Current.ShowUtility();
            Current.position = new Rect(activatorRect.x, activatorRect.y, 300, 400);
        }

        private void OnGUI()
        {
            HandleWindowDrag();
            
            EditorGUILayout.BeginVertical("box");
            DrawTitle("🧱 Select Block");
            DrawBlockList();
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical("box");
            DrawTitle("📦 Selected Cells");
            DrawSelectedCellsList();
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical("box");
            DrawTitle("↻ Rotation");
            DrawRotationButtons();
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
            DrawClearButton();
        }

        private void HandleWindowDrag()
        {
            Rect dragRect = new Rect(0, 0, position.width, 20);
            EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.MoveArrow);

            switch (Event.current.type)
            {
                case EventType.MouseDown when dragRect.Contains(Event.current.mousePosition):
                    _isDragging = true;
                    _dragOffset = Event.current.mousePosition;
                    Event.current.Use();
                    break;
                case EventType.MouseUp:
                    _isDragging = false;
                    break;
                case EventType.MouseDrag when _isDragging:
                {
                    var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
                    position = new Rect(mousePos.x - _dragOffset.x, mousePos.y - _dragOffset.y, position.width, position.height);
                    Event.current.Use();
                    break;
                }
            }
        }
        
        private void OnLostFocus()
        {
            Close();
        }
        
        private void DrawTitle(string title)
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter
            };
            GUILayout.Label(title, style);
        }

        private void DrawBlockList()
        {
            GUILayout.BeginVertical();

            _searchBlockFilter = EditorGUILayout.TextField("Search Block", _searchBlockFilter);

            _scrollBlocks = GUILayout.BeginScrollView(_scrollBlocks, false, true);

            if (_library != null)
            {
                foreach (var block in _library.AllBlocks)
                {
                    if (!string.IsNullOrEmpty(_searchBlockFilter) &&
                        !block.ID.ToLower().Contains(_searchBlockFilter.ToLower()))
                        continue;

                    GUILayout.BeginHorizontal("box");

                    if (block.Icon != null)
                        GUILayout.Label(block.Icon.texture, GUILayout.Width(32), GUILayout.Height(32));

                    GUILayout.Label(block.ID);

                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Set", GUILayout.Width(60)))
                        _onBlockSelected?.Invoke(block);

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }


        private void DrawSelectedCellsList()
        {
            _showSelectionList = EditorGUILayout.Foldout(_showSelectionList, "🟩 Selected Cells Details", true);
            if (!_showSelectionList) return;

            _searchFilter = EditorGUILayout.TextField("Search", _searchFilter);

            _scrollSelection = EditorGUILayout.BeginScrollView(_scrollSelection, GUILayout.Height(150));
            EditorGUILayout.BeginVertical("box");

            if (_currentSelection != null)
            {
                foreach (var pos in _currentSelection)
                {
                    var cell = LevelEditorHelpers.TryGetCell(pos);
                    if (cell == null) continue;

                    string blockId = cell.Block?.ID ?? "Empty";
                    string label = $"[{pos.x},{pos.y}] - {blockId}";

                    if (string.IsNullOrEmpty(_searchFilter) || label.ToLower().Contains(_searchFilter.ToLower()))
                    {
                        EditorGUILayout.BeginHorizontal();

                        if (cell.Block?.Icon != null)
                            GUILayout.Label(cell.Block.Icon.texture, GUILayout.Width(32), GUILayout.Height(32));
                        else
                            GUILayout.Label("❌", GUILayout.Width(32), GUILayout.Height(32));

                        EditorGUILayout.LabelField(label);
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawRotationButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("↑\n0°")) _onRotationSelected?.Invoke(0);
            if (GUILayout.Button("→\n90°")) _onRotationSelected?.Invoke(90);
            if (GUILayout.Button("↓\n180°")) _onRotationSelected?.Invoke(180);
            if (GUILayout.Button("←\n270°")) _onRotationSelected?.Invoke(270);
            GUILayout.EndHorizontal();
        }

        private void DrawClearButton()
        {
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f); 
            if (GUILayout.Button("🗑 Clear Block(s)", GUILayout.Height(30)))
            {
                _onClear?.Invoke();
            }
            GUI.backgroundColor = Color.white;
        }

        public static class LevelEditorHelpers
        {
            public static Func<Vector2Int, LevelCell> TryGetCell = _ => null;
        }
        
        private void OnDestroy()
        {
            _currentSelection?.Clear();

            if (LevelEditorHelpers.TryGetCell != null)
                LevelEditorHelpers.TryGetCell = _ => null;

            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            
            if (Current == this)
                Current = null;
        }
    }
}
#endif