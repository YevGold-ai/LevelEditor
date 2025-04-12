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
        [LabelText("Block ID")] [ShowInInspector]
        private string _newBlockId = "";
        [BoxGroup("🧱 Create Block")] 
        [LabelText("Icon")] [ShowInInspector]
        private Sprite _newBlockSprite;
        [BoxGroup("🧱 Create Block")] 
        [LabelText("Prefab")] [ShowInInspector]
        private GameObject _newBlockPrefab;

        [BoxGroup("🔍 Search & Sort", centerLabel: true)] 
        [LabelText("Filter")] [ShowInInspector]
        private string _searchFilter = "";
        [BoxGroup("🔍 Search & Sort")]
        [HorizontalGroup("🔍 Search & Sort/SortRow")]
        [EnumToggleButtons, HideLabel]
        [PropertyOrder(0)]
        [SerializeField]
        private SortMode _sortMode = SortMode.ByID;

        private bool _sortAscending = true;
        private Vector2 _scroll;

        private enum SortMode
        {
            ByID,
            ByPrefabName,
            ByIconName
        }

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

        [HorizontalGroup("🔍 Search & Sort/SortRow", width: 25)]
        [Button("@_sortAscending ? \"▲\" : \"▼\"", ButtonSizes.Medium)]
        [PropertyOrder(1)]
        private void ToggleSortDirection()
        {
            _sortAscending = !_sortAscending;
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

            blocks.Sort((a, b) =>
            {
                switch (_sortMode)
                {
                    case SortMode.ByID: return string.Compare(a.ID, b.ID, StringComparison.OrdinalIgnoreCase);
                    case SortMode.ByIconName:
                        return string.Compare(a.Icon?.name ?? "", b.Icon?.name ?? "",
                            StringComparison.OrdinalIgnoreCase);
                    case SortMode.ByPrefabName:
                        return SafeCompare(a.Prefab, b.Prefab);
                    default: return 0;
                }
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

                GUILayout.Label(block.Icon != null ? block.Icon.texture : Texture2D.grayTexture, GUILayout.Width(32),
                    GUILayout.Height(32));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                    Selection.activeObject = block;

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Block", $"Are you sure you want to delete '{block.ID}'?",
                            "Yes", "No"))
                    {
                        string path = AssetDatabase.GetAssetPath(block);
                        _blockLibrary.AllBlocks.Remove(block);
                        EditorUtility.SetDirty(_blockLibrary);
                        AssetDatabase.DeleteAsset(path);
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
            var folderPath = "Assets/Resources/StaticData/BlocksData";
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
                string nameA = a != null ? a.name : string.Empty;
                string nameB = b != null ? b.name : string.Empty;
                return string.Compare(nameA, nameB, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return 0;
            }
        }
    }
}