using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Code.LevelEditor
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "StaticData/Levels/Block", order = 802)]
    public class BlockDataEditor : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject prefab;

        public string ID => id;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;

        public void SetID(string newId)
        {
            id = newId;
            this.name = newId;

#if UNITY_EDITOR
            string assetPath = AssetDatabase.GetAssetPath(this);
            if (!string.IsNullOrEmpty(assetPath))
            {
                AssetDatabase.RenameAsset(assetPath, newId);
            }
#endif
        }

        public void SetIcon(Sprite newIcon)
        {
            icon = newIcon;
        }

        public void SetPrefab(GameObject newPrefab)
        {
            prefab = newPrefab;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(id) ? "Unnamed Block" : id;
        }

        public override bool Equals(object obj)
        {
            return obj is BlockDataEditor other && id == other.id;
        }

        public override int GetHashCode()
        {
            return id != null ? id.GetHashCode() : 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(id))
            {
                SetID(id);
            }
        }
#endif
    }
}