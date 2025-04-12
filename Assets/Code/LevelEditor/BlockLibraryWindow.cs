using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

namespace Code.LevelEditor.Editor
{
    public class BlockLibraryWindow : OdinEditorWindow
    {
        private enum SortMode { ByID, ByPrefabName, ByIconName }

        [MenuItem("Tools/Block Library 🧱")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlockLibraryWindow>(false, "Block Library", true);
            window.minSize = new Vector2(400, 500);
        }

        [BoxGroup("📦 Block Library", centerLabel: true)]
        [ReadOnly, ShowInInspector, HideLabel]
        private BlockLibrary _blockLibrary;

        [BoxGroup("🧱 Create Block", centerLabel: true)]
        [ShowInInspector, LabelText("Block ID")]
        private string _newBlockId = "";

        [BoxGroup("🧱 Create Block")]
        [ShowInInspector, LabelText("Icon")]
        private Sprite _newBlockSprite;

        [BoxGroup("🧱 Create Block")]
        [ShowInInspector, LabelText("Prefab")]
        private GameObject _newBlockPrefab;

        [BoxGroup("🔍 Search & Sort", centerLabel: true)]
        [ShowInInspector, LabelText("Filter")]
        private string _searchFilter = "";

        [BoxGroup("🔍 Search & Sort")]
        [HorizontalGroup("🔍 Search & Sort/SortRow")]
        [EnumToggleButtons, HideLabel, PropertyOrder(0)]
        [SerializeField]
        private SortMode _sortMode = SortMode.ByID;

        [HorizontalGroup("🔍 Search & Sort/SortRow", width: 25)]
        [Button("@_sortAscending ? \"▲\" : \"▼\"", ButtonSizes.Medium)]
        [PropertyOrder(1)]
        private void ToggleSortDirection() => _sortAscending = !_sortAscending;

        private bool _sortAscending = true;
        private Vector2 _scroll;

        [BoxGroup("🧾 Selected Block", centerLabel: true)]
        [ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.Hidden)]
        [PropertyOrder(100)]
        private BlockDataEditor _selectedBlock;

        protected override void OnEnable()
        {
            base.OnEnable();
            LoadOrCreateLibrary();
        }

        protected override void DrawEditor(int index)
        {
            base.DrawEditor(index);

            if (_blockLibrary == null)
            {
                SirenixEditorGUI.ErrorMessageBox("No BlockLibrary asset found.");
                if (GUILayout.Button("Create BlockLibrary"))
                {
                    CreateLibrary();
                }
                return;
            }

            GUILayout.Space(10);

            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            SirenixEditorGUI.Title("📦 Existing Blocks", null, TextAlignment.Center, true);
            SirenixEditorGUI.EndBoxHeader();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawExistingBlocks();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();

            SirenixEditorGUI.EndBox();
        }

        private void LoadOrCreateLibrary()
        {
            var guids = AssetDatabase.FindAssets("t:BlockLibrary");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
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
            const string folderPath = "Assets/Resources/StaticData/BlocksData";
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources/StaticData", "BlocksData");

            var path = $"{folderPath}/BlockLibrary.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _blockLibrary = asset;
            Debug.Log("✅ BlockLibrary created at " + path);
        }

        private void DrawExistingBlocks()
        {
            if (_blockLibrary.AllBlocks == null || _blockLibrary.AllBlocks.Count == 0)
            {
                SirenixEditorGUI.InfoMessageBox("No blocks found.");
                return;
            }

            var blocks = new List<BlockDataEditor>(_blockLibrary.AllBlocks);

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                blocks = blocks.FindAll(b => b != null && b.ID.ToLower().Contains(_searchFilter.ToLower()));
            }

            blocks.Sort((a, b) => _sortMode switch
            {
                SortMode.ByID => string.Compare(a.ID, b.ID, StringComparison.OrdinalIgnoreCase),
                SortMode.ByIconName => string.Compare(a.Icon?.name ?? "", b.Icon?.name ?? "", StringComparison.OrdinalIgnoreCase),
                SortMode.ByPrefabName => SafeCompare(a.Prefab, b.Prefab),
                _ => 0
            });

            if (!_sortAscending)
                blocks.Reverse();

            foreach (var block in blocks)
            {
                if (block == null) continue;

                SirenixEditorGUI.BeginBox();
                SirenixEditorGUI.BeginBoxHeader();
                GUILayout.Label(block.ID);
                SirenixEditorGUI.EndBoxHeader();

                GUILayout.BeginHorizontal();
                GUILayout.Label(block.Icon != null ? block.Icon.texture : Texture2D.grayTexture, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    _selectedBlock = block;
                    Selection.activeObject = block;
                }

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Block", $"Are you sure you want to delete '{block.ID}'?", "Yes", "No"))
                    {
                        var path = AssetDatabase.GetAssetPath(block);
                        _blockLibrary.AllBlocks.Remove(block);
                        AssetDatabase.DeleteAsset(path);
                        EditorUtility.SetDirty(_blockLibrary);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        GUIUtility.ExitGUI();
                    }
                }

                GUILayout.EndHorizontal();
                SirenixEditorGUI.EndBox();
            }
        }

        [BoxGroup("🧱 Create Block")]
        [GUIColor(0.2f, 0.8f, 0.2f)]
        [Button("🚀 Create New Block", ButtonSizes.Large)]
        [EnableIf("@!string.IsNullOrEmpty(_newBlockId) && _newBlockSprite != null")]
        private void CreateNewBlock()
        {
            const string folderPath = "Assets/Resources/StaticData/BlocksData";
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources/StaticData", "BlocksData");

            var newBlock = CreateInstance<BlockDataEditor>();
            newBlock.name = _newBlockId;
            newBlock.SetID(_newBlockId);
            newBlock.SetIcon(_newBlockSprite);
            newBlock.SetPrefab(_newBlockPrefab);

            var assetPath = $"{folderPath}/{_newBlockId}.asset";
            AssetDatabase.CreateAsset(newBlock, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _blockLibrary.AllBlocks.Add(newBlock);
            EditorUtility.SetDirty(_blockLibrary);
            AssetDatabase.SaveAssets();

            Debug.Log($"✅ Created new block: {_newBlockId}");
            _newBlockId = "";
            _newBlockSprite = null;
            _newBlockPrefab = null;
        }

        private int SafeCompare(UnityEngine.Object a, UnityEngine.Object b)
        {
            try
            {
                return string.Compare(a?.name ?? string.Empty, b?.name ?? string.Empty, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return 0;
            }
        }
    }
}
