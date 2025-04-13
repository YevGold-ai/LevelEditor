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
            var levelData = _staticDataService.GetLevelData(_currentLevelIndex.ToString());
            int width = levelData.Cells.GetLength(0);
            int height = levelData.Cells.GetLength(1);

            _tileMatrix = new Tile[width, height];
            TileBalanceData tileBalanceData = _staticDataService.Balance.TileBalanceData;

            float baseDelay = 0.02f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int invertedY = height - 1 - y; // для перевёрнутого рендера

                    Vector3 spawnPosition = new Vector3(
                        x - width / 2,
                        0f,
                        y - height / 2
                    );

                    Tile tile = _tileFactory.CreateTile(_rootMapHolder);
                    tile.transform.localPosition = spawnPosition;

                    bool isEven = (x + y) % 2 == 0;
                    Color tileColor = isEven ? tileBalanceData.ColorEven : tileBalanceData.ColorNotEven;
                    tile.SetColor(tileColor);
                    tile.SetUpScale();

                    float delay = (x + y) * baseDelay;
                    DOVirtual.DelayedCall(delay, tile.PlayAnimationShowTile);

                    LevelCell cellData = levelData.Cells[x, invertedY];
                    if (cellData != null && cellData.Block != null)
                    {
                        GameObject block = _blockFactory.CreateBlock(cellData.Block.Prefab, tile.gameObject);
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