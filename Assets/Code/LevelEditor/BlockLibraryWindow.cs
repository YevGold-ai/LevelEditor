using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace Code.LevelEditor.Editor
{
    public class BlockLibraryWindow : EditorWindow
    {
        private BlockLibrary _blockLibrary;

        private string _newBlockId = "";
        private Sprite _newBlockSprite;
        private GameObject _newBlockPrefab;

        private Vector2 _scrollPosition;
        private string _searchFilter = "";

        private enum SortMode { ByID, ByPrefabName, ByIconName }
        private SortMode _sortMode = SortMode.ByID;
        private bool _sortAscending = true;

        [MenuItem("Tools/Block Library 🧱")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlockLibraryWindow>(false, "Block Library", true);
            window.minSize = new Vector2(350, 400);
        }

        private void OnEnable()
        {
            LoadOrCreateLibrary();
        }

        private void OnGUI()
        {
            if (_blockLibrary == null)
            {
                EditorGUILayout.HelpBox("No BlockLibrary asset found.", MessageType.Warning);
                if (GUILayout.Button("Create BlockLibrary"))
                {
                    CreateLibrary();
                }
                return;
            }

            DrawSearchField();
            DrawSortControls();

            GUILayout.Space(10);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawExistingBlocks();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(15);
            DrawCreateBlockSection();
        }

        private void LoadOrCreateLibrary()
        {
            string[] guids = AssetDatabase.FindAssets("t:BlockLibrary");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _blockLibrary = AssetDatabase.LoadAssetAtPath<BlockLibrary>(path);
            }
            else
            {
                CreateLibrary();
            }
        }

        private void CreateLibrary()
        {
            var asset = CreateInstance<BlockLibrary>();
            string folderPath = "Assets/Resources/StaticData/BlocksData";
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources/StaticData", "BlocksData");

            string path = $"{folderPath}/BlockLibrary.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _blockLibrary = asset;
            Debug.Log("✅ BlockLibrary created at " + path);
        }

        private void DrawSearchField()
        {
            GUILayout.Label("Search Blocks", EditorStyles.boldLabel);
            _searchFilter = EditorGUILayout.TextField("Filter", _searchFilter);
        }

        private void DrawSortControls()
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Sort By:", GUILayout.Width(60));
            _sortMode = (SortMode)EditorGUILayout.EnumPopup(_sortMode);

            string arrow = _sortAscending ? "▲" : "▼";
            if (GUILayout.Button(arrow, GUILayout.Width(25)))
            {
                _sortAscending = !_sortAscending;
            }
            GUILayout.EndHorizontal();
        }

        private void DrawExistingBlocks()
        {
            GUILayout.Label("Existing Blocks", EditorStyles.boldLabel);

            if (_blockLibrary.AllBlocks == null || _blockLibrary.AllBlocks.Count == 0)
            {
                GUILayout.Label("No blocks found.");
                return;
            }

            List<BlockDataEditor> blocksToShow = new List<BlockDataEditor>(_blockLibrary.AllBlocks);

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                blocksToShow = blocksToShow.FindAll(block =>
                    block != null && block.ID.ToLower().Contains(_searchFilter.ToLower()));
            }

            SortBlocks();

            if (!_sortAscending)
                blocksToShow.Reverse();

            for (int i = blocksToShow.Count - 1; i >= 0; i--)
            {
                var block = blocksToShow[i];
                if (block == null)
                {
                    _blockLibrary.AllBlocks.RemoveAt(i);
                    continue;
                }

                EditorGUILayout.BeginHorizontal("box");

                GUILayout.Label(block.Icon != null ? block.Icon.texture : Texture2D.grayTexture,
                    GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Label(block.ID);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                    Selection.activeObject = block;

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Block", $"Are you sure you want to delete '{block.ID}'?", "Yes", "No"))
                    {
                        string path = AssetDatabase.GetAssetPath(block);
                        _blockLibrary.AllBlocks.Remove(block);
                        AssetDatabase.DeleteAsset(path);
                        EditorUtility.SetDirty(_blockLibrary);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        GUIUtility.ExitGUI();
                    }
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private void SortBlocks()
        {
            _blockLibrary.AllBlocks.Sort((a, b) =>
            {
                switch (_sortMode)
                {
                    case SortMode.ByID:
                        return string.Compare(a.ID, b.ID, StringComparison.OrdinalIgnoreCase);
                    case SortMode.ByIconName:
                        var aSprite = a.Icon != null ? a.Icon.name : string.Empty;
                        var bSprite = b.Icon != null ? b.Icon.name : string.Empty;
                        return string.Compare(aSprite, bSprite, StringComparison.OrdinalIgnoreCase);
                    case SortMode.ByPrefabName:
                        var aPrefab = a.Prefab != null ? a.Prefab.name : string.Empty;
                        var bPrefab = b.Prefab != null ? b.Prefab.name : string.Empty;
                        return string.Compare(aPrefab, bPrefab, StringComparison.OrdinalIgnoreCase);
                    default:
                        return 0;
                }
            });
        }
        
        private void DrawCreateBlockSection()
        {
            GUILayout.Label("Create New Block", EditorStyles.boldLabel);

            _newBlockId = EditorGUILayout.TextField("ID", _newBlockId);
            _newBlockSprite = (Sprite)EditorGUILayout.ObjectField("Sprite", _newBlockSprite, typeof(Sprite), false);
            _newBlockPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", _newBlockPrefab, typeof(GameObject), false);

            GUI.enabled = !string.IsNullOrEmpty(_newBlockId) && _newBlockSprite != null;
            if (GUILayout.Button("Create Block", GUILayout.Height(30)))
            {
                CreateNewBlock();
            }
            GUI.enabled = true;
        }

        private void CreateNewBlock()
        {
            if (_blockLibrary == null)
            {
                Debug.LogError("BlockLibrary not assigned or found.");
                return;
            }

            string folderPath = "Assets/Resources/StaticData/BlocksData";
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources/StaticData", "BlocksData");

            BlockDataEditor newBlock = CreateInstance<BlockDataEditor>();
            newBlock.name = _newBlockId;
            newBlock.SetID(_newBlockId);
            newBlock.SetIcon(_newBlockSprite);
            newBlock.SetPrefab(_newBlockPrefab);

            string assetPath = $"{folderPath}/{_newBlockId}.asset";
            AssetDatabase.CreateAsset(newBlock, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _blockLibrary.AllBlocks.Add(newBlock);
            EditorUtility.SetDirty(_blockLibrary);

            Debug.Log($"✅ Created new block: {_newBlockId}");
            _newBlockId = "";
            _newBlockSprite = null;
            _newBlockPrefab = null;
        }
    }
}
