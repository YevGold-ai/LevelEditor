using UnityEngine;
using Zenject;

namespace Code.Infrastructure.Generator.Factory
{
    public class BlockFactory : Infrastructure.Factory.Factory, IBlockFactory
    {
        private const string TilePrefabPath = "Tile/Tile";
        
        public BlockFactory(IInstantiator instantiator) : base(instantiator)
        {
            
        }
        
        public GameObject CreateBlock(GameObject prefab,GameObject parent)
        {
            var blockGameObject = Instantiate(prefab, parent.transform);
            return blockGameObject;
        }
    }
}