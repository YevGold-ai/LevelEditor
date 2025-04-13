using UnityEngine;

namespace Code.Infrastructure.Generator.Factory
{
    public interface ITileFactory
    {
        Tile CreateTile(GameObject parent);
    }
}