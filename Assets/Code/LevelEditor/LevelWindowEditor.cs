using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Code.LevelEditor.Editor
{
    public class LevelWindowEditor : OdinEditorWindow
    {
        [MenuItem("Tools/Level Editor 🚀")]
        private static void OpenWindow()
        {
            var window = GetWindow<LevelWindowEditor>();
            window.titleContent = new GUIContent("Level Editor");
            window.Show();
        }

        [ReadOnly]
        [SerializeField]
        private List<BaseLevelDataEditor> levelEditors = new();
        [BoxGroup("Select Level")]
        [ValueDropdown(nameof(GetLevelEditorNames))]
        [OnValueChanged(nameof(SetSelectedLevelEditor))]
        public string SelectedName;
        [BoxGroup("Select Level")]
        [Space(10)]
        [InlineEditor(InlineEditorModes.GUIOnly)]
        [ShowIf(nameof(SelectedLevelEditor))]
        public BaseLevelDataEditor SelectedLevelEditor;
        [BoxGroup("🟩 Create Level", centerLabel: true)]
        [GUIColor(0.8f, 1f, 0.8f)]
        public string NewLevelName = "NewLevel";
        [BoxGroup("🟩 Create Level")]
        [GUIColor(0.8f, 1f, 0.8f)]
        public int NewLevelWidth = 5;
        [BoxGroup("🟩 Create Level")]
        [GUIColor(0.8f, 1f, 0.8f)]
        public int NewLevelHeight = 5;
        [BoxGroup("🟩 Create Level")]
        [GUIColor(0.6f, 1f, 0.6f)]
        [EnumToggleButtons]
        public LevelType SelectedLevelType;
        [BoxGroup("🟩 Create Level")]
        [GUIColor(0.2f, 0.8f, 0.2f)]
        [Button("🚀 Create Level Editor", ButtonSizes.Large)]
        
        private void CreateNewLevelEditor()
        {
            BaseLevelDataEditor newEditor = null;

            if (SelectedLevelType == LevelType.Matrix)
            {
                var matrixEditor = CreateInstance<LevelMatrixEditor>();
                matrixEditor.Grid = new LevelCell[NewLevelWidth, NewLevelHeight];
                for (int y = 0; y < NewLevelHeight; y++)
                for (int x = 0; x < NewLevelWidth; x++)
                    matrixEditor.Grid[x, y] = new LevelCell();
                newEditor = matrixEditor;
            }
            else if (SelectedLevelType == LevelType.Hexagon)
            {
                var hexEditor = CreateInstance<LevelHegsagonEditor>();
                hexEditor.Grid = new LevelCell[NewLevelWidth, NewLevelHeight];
                for (int y = 0; y < NewLevelHeight; y++)
                for (int x = 0; x < NewLevelWidth; x++)
                    hexEditor.Grid[x, y] = new LevelCell();
                newEditor = hexEditor;
            }

            if (newEditor == null) return;

            string folderPath = "Assets/Resources/StaticData/LevelsData";
            if (!AssetDatabase.IsValidFolder(folderPath))
                AssetDatabase.CreateFolder("Assets/Resources/StaticData", "LevelsData");

            string assetPath = $"{folderPath}/{NewLevelName}.asset";
            AssetDatabase.CreateAsset(newEditor, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"✅ Created new level editor: {assetPath}");

            LoadLevelEditors();
            SelectedLevelEditor = newEditor;
            SelectedName = NewLevelName;
        }

        private void OnEnable()
        {
            LoadLevelEditors();
            if (levelEditors.Count > 0)
            {
                SelectedLevelEditor = levelEditors[0];
                SelectedName = SelectedLevelEditor.name;
            }
        }

        private void LoadLevelEditors()
        {
            levelEditors.Clear();

            string searchFolder = "Assets/Resources/StaticData/LevelsData";
            string[] guids = AssetDatabase.FindAssets("t:BaseLevelDataEditor", new[] { searchFolder });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var editorAsset = AssetDatabase.LoadAssetAtPath<BaseLevelDataEditor>(path);
                if (editorAsset != null)
                    levelEditors.Add(editorAsset);
            }
        }

        private IEnumerable<string> GetLevelEditorNames()
        {
            foreach (var editor in levelEditors)
                yield return editor.name;
        }

        private void SetSelectedLevelEditor()
        {
            SelectedLevelEditor = levelEditors.Find(e => e.name == SelectedName);
        }

        public enum LevelType
        {
            Matrix,
            Hexagon
        }
    }
}
