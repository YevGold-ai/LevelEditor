using Sirenix.OdinInspector;
using UnityEngine;

namespace Code.LevelEditor
{
    public abstract class BaseLevelDataEditor : SerializedScriptableObject
    {
        public abstract LevelCell GetCell(Vector2Int pos);
    }
}