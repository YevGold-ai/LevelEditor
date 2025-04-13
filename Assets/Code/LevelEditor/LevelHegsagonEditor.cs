using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

namespace Code.LevelEditor
{
    [CreateAssetMenu(menuName = "StaticData/Levels/LevelData", fileName = "LevelData", order = 801)]
    public class LevelHegsagonEditor : BaseLevelDataEditor
    {
        [HorizontalGroup("Level")]
        [OnValueChanged(nameof(ResizeGrid))]
        [SerializeField, LabelText("Level Index")]
        public int IndexLevel;
        
        [HorizontalGroup("Size")]
        [OnValueChanged(nameof(ResizeGrid))]
        [SerializeField, LabelText("Width")]
        private int width = 5;

        [HorizontalGroup("Size")]
        [OnValueChanged(nameof(ResizeGrid))]
        [SerializeField, LabelText("Height")]
        private int height = 5;

        [OdinSerialize]
        [TableMatrix(
            HorizontalTitle = "Level Grid",
            DrawElementMethod = nameof(DrawLevelCell),
            ResizableColumns = false,
            Transpose = true)]
        public LevelCell[,] Grid;

#if UNITY_EDITOR

        private static BlockLibrary _cachedLibrary;
        private Vector2Int? _selectionStart;
        private List<Vector2Int> _currentSelection = new();

        private LevelCell DrawLevelCell(Rect rect, LevelCell cell, int x, int y)
        {
            cell ??= new LevelCell();
            LoadLibrary();

            if (y % 2 != 0)
                rect.x += rect.width / 2;
            
            DrawHexagonOutline(rect);

            DrawHexBackground(rect);
            DrawCellContent(rect, GetCell(x, y));
            DrawSelectionOverlay(rect, x, y);
            HandleCellInteraction(rect, x, y);

            return GetCell(x, y);
        }

        private void DrawHexBackground(Rect rect)
        {
            Handles.BeginGUI();
            Handles.color = new Color(0.25f, 0.25f, 0.25f, 0.8f);
            Vector2 center = rect.center;
            float size = rect.width / 2.3f;

            Vector3[] hex = new Vector3[7];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60 * i);
                hex[i] = new Vector3(center.x + size * Mathf.Cos(angle), center.y + size * Mathf.Sin(angle));
            }
            hex[6] = hex[0];
            Handles.DrawAAConvexPolygon(hex);
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        private void LoadLibrary()
        {
            if (_cachedLibrary != null) return;
            string[] guids = AssetDatabase.FindAssets("t:BlockLibrary");
            if (guids.Length == 0) return;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _cachedLibrary = AssetDatabase.LoadAssetAtPath<BlockLibrary>(path);
        }

        private void DrawCellContent(Rect rect, LevelCell cell)
        {
            if (cell.Block != null && cell.Block.Icon != null)
            {
                GUI.DrawTexture(rect.Padding(4), cell.Block.Icon.texture, ScaleMode.ScaleToFit);
                DrawBlockId(rect, cell.Block.ID);
                DrawRotationArrow(rect, cell.Rotation);
            }
            else
            {
                //  EditorGUI.DrawRect(rect.Padding(4), Color.gray);
            }
        }

        private void DrawHexagonOutline(Rect rect)
        {
            Vector2 center = rect.center;
            
            float radius = rect.height / 2f * 0.85f;

            Vector3[] points = new Vector3[7];
            for (int i = 0; i < 6; i++)
            {
                float angle = Mathf.Deg2Rad * (60 * i - 30);
                points[i] = new Vector3(
                    center.x + radius * Mathf.Cos(angle),
                    center.y + radius * Mathf.Sin(angle),
                    0f
                );
            }

            points[6] = points[0];

            Handles.color = new Color(1f, 1f, 1f, 0.2f);
            Handles.DrawPolyLine(points);
        }
        
        private void DrawBlockId(Rect rect, string id)
        {
            GUI.Label(rect.Padding(2), id, new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.LowerCenter,
                fontSize = 12,
                normal = { textColor = Color.white }
            });
        }

        private void DrawRotationArrow(Rect rect, Quaternion rotation)
        {
            var oldMatrix = GUI.matrix;
            Vector2 center = rect.center;
            GUIUtility.RotateAroundPivot(rotation.eulerAngles.z, center);
            GUI.Label(new Rect(center.x - 10, rect.yMin - 4, 25, 25), "↑", new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.UpperCenter,
                fontSize = 25,
                normal = { textColor = Color.green },
                fontStyle = FontStyle.Bold
            });
            GUI.matrix = oldMatrix;
        }

        private void DrawSelectionOverlay(Rect rect, int x, int y)
        {
            if (_currentSelection.Contains(new Vector2Int(x, y)))
            {
                EditorGUI.DrawRect(rect, new Color(0f, 1f, 0f, 0.25f));
            }
        }

        private void HandleCellInteraction(Rect rect, int x, int y)
        {
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                if (Event.current.button == 0)
                {
                    if (Event.current.control)
                    {
                        if (!_selectionStart.HasValue)
                        {
                            _selectionStart = new Vector2Int(x, y);
                            _currentSelection.Clear();
                            _currentSelection.Add(_selectionStart.Value);
                        }
                        else
                        {
                            Vector2Int end = new Vector2Int(x, y);
                            _currentSelection.Clear();
                            int minX = Mathf.Min(_selectionStart.Value.x, end.x);
                            int maxX = Mathf.Max(_selectionStart.Value.x, end.x);
                            int minY = Mathf.Min(_selectionStart.Value.y, end.y);
                            int maxY = Mathf.Max(_selectionStart.Value.y, end.y);

                            for (int i = minX; i <= maxX; i++)
                            for (int j = minY; j <= maxY; j++)
                                _currentSelection.Add(new Vector2Int(i, j));

                            _selectionStart = null;
                            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                        }
                    }
                    else
                    {
                        _selectionStart = null;
                        _currentSelection.Clear();
                    }

                    Event.current.Use();
                }

                if (Event.current.button == 1)
                {
                    ShowContextMenu(GetCell(x, y), x, y);
                    Event.current.Use();
                }
            }
        }

        private void ShowContextMenu(LevelCell cell, int x, int y)
        {
            if (_cachedLibrary == null || cell == null)
                return;

            Vector2 screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Rect rect = new Rect(screenPos, new Vector2(350, 500));
            
            Vector2Int clickedPos = new Vector2Int(x, y);
            if (!_currentSelection.Contains(clickedPos))
            {
                Debug.Log("Already selected");
                Debug.Log(_currentSelection.Contains(clickedPos));
                _currentSelection.Add(clickedPos);
            }
            else
            {
               Debug.Log("Dont Already selected");
            }
            
            BlockPopupWindow.LevelEditorHelpers.TryGetCell = pos => GetCell(pos);

            BlockPopupWindow.ShowPopup(
                rect,
                _cachedLibrary,
                block => ApplyBlock(block, cell),
                angle => ApplyRotation(angle, cell),
                () => ClearBlock(cell),
                _currentSelection
            );
        }


        private void ApplyBlock(BlockDataEditor block, LevelCell fallbackCell)
        {
            if (_currentSelection.Count > 0)
            {
                foreach (var pos in _currentSelection)
                    GetCell(pos).Block = block;
            }
            else fallbackCell.Block = block;
            MarkDirty();
        }

        private void ApplyRotation(float angle, LevelCell fallbackCell)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            if (_currentSelection.Count > 0)
            {
                foreach (var pos in _currentSelection)
                    GetCell(pos).Rotation = rotation;
            }
            else fallbackCell.Rotation = rotation;
            MarkDirty();
        }

        private void ClearBlock(LevelCell fallbackCell)
        {
            if (_currentSelection.Count > 0)
            {
                foreach (var pos in _currentSelection)
                    GetCell(pos).Block = null;
                _currentSelection.Clear();
            }
            else fallbackCell.Block = null;
            MarkDirty();
        }

        private void MarkDirty()
        {
            GUI.changed = true;
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        private LevelCell GetCell(int x, int y) => Grid[y, x];
        public override LevelCell GetCell(Vector2Int pos) => Grid[pos.y, pos.x];
#endif

        public IEnumerable<Vector2Int> GetAllOfType(BlockDataEditor type)
        {
            for (int y = 0; y < Grid.GetLength(1); y++)
            for (int x = 0; x < Grid.GetLength(0); x++)
                if (Grid[y, x]?.Block == type)
                    yield return new Vector2Int(x, y);
        }

        private void OnValidate() => ResizeGrid();

        private void ResizeGrid()
        {
            if (width <= 0) width = 1;
            if (height <= 0) height = 1;

            var newGrid = new LevelCell[width, height];

            if (Grid != null)
            {
                int minWidth = Mathf.Min(width, Grid.GetLength(0));
                int minHeight = Mathf.Min(height, Grid.GetLength(1));

                for (int x = 0; x < minWidth; x++)
                for (int y = 0; y < minHeight; y++)
                    newGrid[x, y] = Grid[x, y];
            }

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                newGrid[x, y] ??= new LevelCell();

            Grid = newGrid;
        }
    }
}
