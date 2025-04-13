using Code.Infrastructure.Generator.Factory;
using Code.Infrastructure.Services.StaticData;
using Code.Infrastructure.StaticData;
using Code.LevelEditor;
using DG.Tweening;
using UnityEngine;

namespace Code.Infrastructure.Generator.Services
{
    public class LevelGeneratorService : ILevelGeneratorService
    {
        private Tile[,] _tileMatrix;
        private GameObject _rootMapHolder;
        private int _currentLevelIndex = 1;
        
        private readonly IStaticDataService _staticDataService;
        private readonly ITileFactory _tileFactory;
        private readonly IBlockFactory _blockFactory;

        public LevelGeneratorService(
            IStaticDataService staticDataService, 
            ITileFactory tileFactory,
            IBlockFactory blockFactory)
        {
            _staticDataService = staticDataService;
            _tileFactory = tileFactory;
            _blockFactory = blockFactory;
        }
        
        public void SetUpRootMapHolder(GameObject rootMapHolder)
        {
            _rootMapHolder = rootMapHolder;
        }

        public void CleanUp()
        {
            if (_tileMatrix == null) return;

            foreach (var tile in _tileMatrix)
            {
                if (tile != null)
                {
                    UnityEngine.Object.Destroy(tile.gameObject);
                }
            }

            _tileMatrix = null;
        }

        public void GenerateLevel()
        {
            const int gridSize = 11;
            _tileMatrix = new Tile[gridSize, gridSize];

            TileBalanceData tileBalanceData = _staticDataService.Balance.TileBalanceData;

            float baseDelay = 0.02f;

            var levelData = _staticDataService.GetLevelData(_currentLevelIndex.ToString());
            
            for (int y = 0; y < gridSize; y++)
            {
                for (int x = 0; x < gridSize; x++)
                {
                    Vector3 spawnPosition = new Vector3(
                        x - gridSize / 2,
                        0f,
                        y - gridSize / 2
                    );

                    Tile tile = _tileFactory.CreateTile(_rootMapHolder);
                    tile.transform.localPosition = spawnPosition;
                    
                    bool isEven = (x + y) % 2 == 0;
                    Color tileColor = isEven ? tileBalanceData.ColorEven : tileBalanceData.ColorNotEven;
                    tile.SetColor(tileColor);
                    tile.SetUpScale();
                    float delay = (x + y) * baseDelay;
                    DOVirtual.DelayedCall(delay, tile.PlayAnimationShowTile);

                    LevelCell cellData = levelData.Cells[x, y];
                    if (cellData != null && cellData.Block != null)
                    {
                        GameObject block = _blockFactory.CreateBlock(cellData.Block.Prefab,tile.gameObject);
                        block.transform.rotation = cellData.Rotation;
                        
                        tile.SetBlock(block);
                        DOVirtual.DelayedCall(delay, tile.PlayAnimationShowBlock);
                    }
                    
                    _tileMatrix[x, y] = tile;
                }
            }
        }
    }
}