using Code.LevelEditor;
using StaticData;

namespace Code.Infrastructure.Services.StaticData
{
    public interface IStaticDataService
    {
        public void LoadData();
        
        public BalanceStaticData Balance { get; }
        public LevelDataDTO GetLevelData(string levelId);
    }
}