using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Code.LevelEditor
{
    [CreateAssetMenu(menuName = "StaticData/Levels/LevelData", fileName = "LevelData", order = 801)]
    public class LevelMatrixEditor : BaseLevelDataEditor
    {
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
            RowHeight = 64,
            SquareCells = true,
            Transpose = true)]
        public LevelCell[,] Grid;

#if UNITY_EDITOR

        private static BlockLibrary cachedLibrary;
        private Vector2Int? selectionStart;
        private List<Vector2Int> currentSelection = new();

        private LevelCell DrawLevelCell(Rect rect, LevelCell cell, int x, int y)
        {
            cell ??= new LevelCell();

            LoadLibrary();

            DrawCellContent(rect, GetCell(x, y));
            DrawSelectionOverlay(rect, x, y);
            HandleCellInteraction(rect, x, y);

            return GetCell(x, y);
        }

        private void LoadLibrary()
        {
            if (cachedLibrary != null) return;

            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BlockLibrary");
            if (guids.Length == 0) return;

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            cachedLibrary = UnityEditor.AssetDatabase.LoadAssetAtPath<BlockLibrary>(path);
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
                UnityEditor.EditorGUI.DrawRect(rect.Padding(4), Color.gray);
            }
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
            if (currentSelection.Contains(new Vector2Int(x, y)))
            {
                UnityEditor.EditorGUI.DrawRect(rect, new Color(0f, 1f, 0f, 0.25f));
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
                        if (!selectionStart.HasValue)
                        {
                            selectionStart = new Vector2Int(x, y);
                            currentSelection.Clear();
                            currentSelection.Add(selectionStart.Value);
                        }
                        else
                        {
                            Vector2Int end = new Vector2Int(x, y);
                            currentSelection.Clear();

                            int minX = Mathf.Min(selectionStart.Value.x, end.x);
                            int maxX = Mathf.Max(selectionStart.Value.x, end.x);
                            int minY = Mathf.Min(selectionStart.Value.y, end.y);
                            int maxY = Mathf.Max(selectionStart.Value.y, end.y);

                            for (int i = minX; i <= maxX; i++)
                            for (int j = minY; j <= maxY; j++)
                                currentSelection.Add(new Vector2Int(i, j));

                            selectionStart = null;
                            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                        }
                    }
                    else
                    {
                        selectionStart = null;
                        currentSelection.Clear();
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
            if (cachedLibrary == null) return;

            Vector2 screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Vector2 windowSize = new Vector2(350, 550);
            Rect rect = new Rect(screenPos, windowSize);

            Vector2Int clickedPos = new Vector2Int(x, y);
            if (currentSelection == null || !currentSelection.Contains(clickedPos))
            {
                currentSelection = new List<Vector2Int> { clickedPos };
            }
            
            BlockPopupWindow.LevelEditorHelpers.TryGetCell = pos => GetCell(pos);
            
            BlockPopupWindow.ShowPopup(
                rect,
                cachedLibrary,
                block => ApplyBlock(block, cell),
                angle => ApplyRotation(angle, cell),
                () => ClearBlock(cell),
                currentSelection
            );
        }

        private void ApplyBlock(BlockDataEditor block, LevelCell fallbackCell)
        {
            if (currentSelection.Count > 0)
            {
                foreach (var pos in currentSelection)
                    GetCell(pos).Block = block;
            }
            else
            {
                fallbackCell.Block = block;
            }

            MarkDirty();
        }

        private void ApplyRotation(float angle, LevelCell fallbackCell)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            if (currentSelection.Count > 0)
            {
                foreach (var pos in currentSelection)
                    GetCell(pos).Rotation = rotation;
            }
            else
            {
                fallbackCell.Rotation = rotation;
            }

            MarkDirty();
        }

        private void ClearBlock(LevelCell fallbackCell)
        {
            if (currentSelection.Count > 0)
            {
                foreach (var pos in currentSelection)
                    GetCell(pos).Block = null;

                currentSelection.Clear();
            }
            else
            {
                fallbackCell.Block = null;
            }

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

        private void OnValidate() => ValidateGrid();

        private void ValidateGrid() => ResizeGrid();

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