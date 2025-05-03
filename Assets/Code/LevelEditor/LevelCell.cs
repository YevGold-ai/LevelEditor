using System;
using UnityEngine;

namespace Code.LevelEditor
{
    [Serializable]
    public class LevelCell
    {
        public BlockDataEditor Block;
        public Quaternion Rotation = Quaternion.identity;
    }
}