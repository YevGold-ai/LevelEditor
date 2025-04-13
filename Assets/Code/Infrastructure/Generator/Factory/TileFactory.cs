using UnityEngine;
using Zenject;

namespace Code.Infrastructure.Generator.Factory
{
    public class TileFactory : Infrastructure.Factory.Factory, ITileFactory
    {
        private const string TilePrefabPath = "Tile/Tile";
        
        public TileFactory(IInstantiator instantiator) : base(instantiator)
        {
            
        }
        
        public Tile CreateTile(GameObject parent)
        {
            var tileObject = Instantiate(TilePrefabPath, parent.transform);
            var tile = tileObject.GetComponent<Tile>();
            return tile;
        }
    }
}
