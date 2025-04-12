using UnityEngine;

namespace Code.LevelEditor
{
    [CreateAssetMenu(fileName = "NewBlock", menuName = "StaticData/Levels/Block", order = 802)]
    public class BlockDataEditor : ScriptableObject
    {
        public string ID;
        public Sprite Icon;
        public GameObject Prefab;
    }
}