using UnityEngine;

namespace Code.Infrastructure.Generator.Factory
{
    public interface IBlockFactory
    {
        GameObject CreateBlock(GameObject prefab,GameObject parent);
    }
}