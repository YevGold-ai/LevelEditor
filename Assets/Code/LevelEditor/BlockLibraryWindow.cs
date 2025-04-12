using UnityEditor;
using UnityEngine;
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

        [MenuItem("Tools/Block Library 🧱")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlockLibraryWindow>(false, "Block Library", true);
            window.minSize = new Vector2(350, 400);
        }

        private void OnEnable()
        {
            string[] guids = AssetDatabase.FindAssets("t:BlockLibrary");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _blockLibrary = AssetDatabase.LoadAssetAtPath<BlockLibrary>(path);
            }
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
            GUILayout.Space(10);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            DrawExistingBlocks();
            EditorGUILayout.EndScrollView();

            GUILayout.Space(15);
            DrawCreateBlockSection();
        }

        private void DrawSearchField()
        {
            GUILayout.Label("Search Blocks", EditorStyles.boldLabel);
            _searchFilter = EditorGUILayout.TextField("Filter", _searchFilter);
        }

        private void DrawExistingBlocks()
        {
            GUILayout.Label("Existing Blocks", EditorStyles.boldLabel);

            if (_blockLibrary.AllBlocks == null || _blockLibrary.AllBlocks.Count == 0)
            {
                GUILayout.Label("No blocks found.");
                return;
            }

            BlockDataEditor blockToDelete = null;

            foreach (var block in _blockLibrary.AllBlocks)
            {
                if (block == null) continue;
                if (!string.IsNullOrEmpty(_searchFilter) && !block.ID.ToLower().Contains(_searchFilter.ToLower()))
                    continue;

                EditorGUILayout.BeginHorizontal("box");

                GUILayout.Label(block.Icon != null ? block.Icon.texture : Texture2D.grayTexture, GUILayout.Width(32), GUILayout.Height(32));
                GUILayout.Label(block.ID);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Select", GUILayout.Width(60)))
                    Selection.activeObject = block;

                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    if (EditorUtility.DisplayDialog("Delete Block", $"Are you sure you want to delete '{block.ID}'?", "Yes", "No"))
                    {
                        blockToDelete = block;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            if (blockToDelete != null)
            {
                _blockLibrary.AllBlocks.Remove(blockToDelete);
                DestroyImmediate(blockToDelete, true);
                EditorUtility.SetDirty(_blockLibrary);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
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
            BlockDataEditor newBlock = CreateInstance<BlockDataEditor>();
            newBlock.name = _newBlockId;
            newBlock.SetID(_newBlockId);
            newBlock.SetIcon(_newBlockSprite);
            newBlock.SetPrefab(_newBlockPrefab);

            AssetDatabase.AddObjectToAsset(newBlock, AssetDatabase.GetAssetPath(_blockLibrary));
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(newBlock));

            _blockLibrary.AllBlocks.Add(newBlock);
            EditorUtility.SetDirty(_blockLibrary);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ Created new block: {_newBlockId}");
            _newBlockId = "";
            _newBlockSprite = null;
            _newBlockPrefab = null;
        }

        private void CreateLibrary()
        {
            var asset = CreateInstance<BlockLibrary>();
            string path = "Assets/Resources/StaticData/BlockLibrary.asset";
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _blockLibrary = asset;
            Debug.Log("✅ BlockLibrary created at " + path);
        }
    }
}
