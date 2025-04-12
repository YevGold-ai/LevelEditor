using System.Collections.Generic;
using UnityEngine;

namespace Code.LevelEditor
{
    [CreateAssetMenu(fileName = "BlockLibrary", menuName = "StaticData/Levels/BlockLibrary", order = 803)]
    public class BlockLibrary : ScriptableObject
    {
        public List<BlockDataEditor> AllBlocks;
    }
}