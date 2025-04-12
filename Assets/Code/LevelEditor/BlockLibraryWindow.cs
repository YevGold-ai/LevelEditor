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
        private enum SortMode
        {
            ByID,
            ByPrefabName,
            ByIconName
        }

        [MenuItem("Tools/Block Library 🧱")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlockLibraryWindow>(false, "Block Library", true);
            window.minSize = new Vector2(400, 500);
        }

        [BoxGroup("📦 Block Library", centerLabel: true)] [ReadOnly, ShowInInspector, HideLabel]
        private BlockLibrary _blockLibrary;
        
        [BoxGroup("🧱 Create Block", centerLabel: true)] [ShowInInspector, LabelText("Block ID")]
        private string _newBlockId = "";
        [BoxGroup("🧱 Create Block")] [ShowInInspector, LabelText("Icon")]
        private Sprite _newBlockSprite;
        [BoxGroup("🧱 Create Block")] [ShowInInspector, LabelText("Prefab")]
        private GameObject _newBlockPrefab;

        [BoxGroup("🔍 Search & Sort", centerLabel: true)] [ShowInInspector, LabelText("Filter")]
        private string _searchFilter = "";
        [BoxGroup("🔍 Search & Sort")]
        [HorizontalGroup("🔍 Search & Sort/SortRow")]
        [EnumToggleButtons, HideLabel, PropertyOrder(0)]
        [SerializeField]
        private SortMode _sortMode = SortMode.ByID;

        private BlockDataEditor _selectedBlock;
        private BlockEditorDto _blockDraft = new BlockEditorDto();
        private bool _sortAscending = true;
        private Vector2 _scroll;

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
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawExistingBlocks();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(10);
            DrawSelectedBlockSection();
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

        private void DrawSelectedBlockSection()
        {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            SirenixEditorGUI.Title("🎯 Selected Block", null, TextAlignment.Center, true);
            SirenixEditorGUI.EndBoxHeader();

            if (_selectedBlock == null)
            {
                SirenixEditorGUI.InfoMessageBox("No block selected.");
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(_selectedBlock.Icon != null ? _selectedBlock.Icon.texture : Texture2D.grayTexture,
                    GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Label(_selectedBlock.ID, SirenixGUIStyles.BoldLabel);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(8);

                // Поля редактирования DTO
                _blockDraft.Id = EditorGUILayout.TextField("ID", _blockDraft.Id);
                _blockDraft.Icon = (Sprite)EditorGUILayout.ObjectField("Icon", _blockDraft.Icon, typeof(Sprite), false);
                _blockDraft.Prefab =
                    (GameObject)EditorGUILayout.ObjectField("Prefab", _blockDraft.Prefab, typeof(GameObject), false);

                GUILayout.Space(4);

                if (GUILayout.Button("💾 Apply Changes", GUILayout.Height(25)))
                {
                    _blockDraft.ApplyTo(_selectedBlock);
                }
            }

            SirenixEditorGUI.EndBox();
        }

        [HorizontalGroup("🔍 Search & Sort/SortRow", width: 25)]
        [Button("@_sortAscending ? \"▲\" : \"▼\"", ButtonSizes.Medium)]
        [PropertyOrder(1)]
        private void ToggleSortDirection() => _sortAscending = !_sortAscending;

        private void DrawExistingBlocks()
        {
            SirenixEditorGUI.BeginBox();
            SirenixEditorGUI.BeginBoxHeader();
            SirenixEditorGUI.Title("📦 Existing Blocks", null, TextAlignment.Center, true);
            SirenixEditorGUI.EndBoxHeader();

            if (_blockLibrary.AllBlocks == null || _blockLibrary.AllBlocks.Count == 0)
            {
                SirenixEditorGUI.InfoMessageBox("No blocks found.");
                SirenixEditorGUI.EndBox();
                return;
            }

            var blocks = new List<BlockDataEditor>(_blockLibrary.AllBlocks);

            if (!string.IsNullOrEmpty(_searchFilter))
            {
                blocks = blocks.FindAll(b => b != null && b.ID.ToLower().Contains(_searchFilter.ToLower()));
            }

            blocks.Sort((a, b) =>
            {
                return _sortMode switch
                {
                    SortMode.ByID => string.Compare(a.ID, b.ID, StringComparison.OrdinalIgnoreCase),
                    SortMode.ByIconName => string.Compare(a.Icon?.name ?? "", b.Icon?.name ?? "",
                        StringComparison.OrdinalIgnoreCase),
                    SortMode.ByPrefabName => SafeCompare(a.Prefab, b.Prefab),
                    _ => 0
                };
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
                
                GUILayout.Label(block.Icon != null ? block.Icon.texture : Texture2D.grayTexture,
                    GUILayout.Width(32), GUILayout.Height(32));

                GUILayout.Space(10);
                
                if (block.Prefab != null)
                {
                    GUILayout.Label(AssetPreview.GetAssetPreview(block.Prefab) ?? Texture2D.grayTexture,
                        GUILayout.Width(32), GUILayout.Height(32));
                    GUILayout.Label(block.Prefab.name, GUILayout.Width(100));
                }
                else
                {
                    GUILayout.Label(Texture2D.grayTexture, GUILayout.Width(32), GUILayout.Height(32));
                    GUILayout.Label("None", GUILayout.Width(100));
                }

                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Select", GUILayout.Width(60)))
                {
                    _selectedBlock = block;
                    _blockDraft.LoadFrom(block);
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

            SirenixEditorGUI.EndBox();
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
        }

        private int SafeCompare(UnityEngine.Object a, UnityEngine.Object b)
        {
            try
            {
                return string.Compare(a?.name ?? string.Empty, b?.name ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return 0;
            }
        }
    }
}