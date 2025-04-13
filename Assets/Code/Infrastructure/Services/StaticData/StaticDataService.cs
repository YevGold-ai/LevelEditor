using System.Collections.Generic;
using System.Linq;
using Code.LevelEditor;
using StaticData;
using UnityEngine;

namespace Code.Infrastructure.Services.StaticData
{
    public class StaticDataService : IStaticDataService
    {
        private const string BalanceStaticDataPath = "StaticData/Balance/Balance";
        private const string LevelStaticDataPath = "StaticData/LevelsData/";

        private BalanceStaticData _balanceStaticData;
        private List<LevelMatrixEditor> _levelMatrixEditors;
        
        public BalanceStaticData Balance => _balanceStaticData;

        public void LoadData()
        {
            _balanceStaticData = Resources.Load<BalanceStaticData>(BalanceStaticDataPath);
            
            _levelMatrixEditors = Resources.LoadAll<LevelMatrixEditor>(LevelStaticDataPath).ToList();
        }
        
        public LevelDataDTO GetLevelData(string levelId)
        {
            if (_levelMatrixEditors == null || _levelMatrixEditors.Count == 0)
            {
                Debug.LogWarning("⚠️ Level data not loaded or empty!");
                return default;
            }
            
            int parsedIndex = ExtractIndexFromId(levelId);
            int loopedIndex = parsedIndex % _levelMatrixEditors.Count;
            
            LevelMatrixEditor selectedEditor = _levelMatrixEditors
                .OrderBy(e => e.IndexLevel)
                .ElementAt(loopedIndex);

            return selectedEditor.GetLevelDataDto();
        }

        private int ExtractIndexFromId(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
                return 0;

            var parts = levelId.Split('_');
            if (parts.Length == 2 && int.TryParse(parts[1], out int result))
                return result;

            return 0;
        }
    }
}