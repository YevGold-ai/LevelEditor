using UnityEngine;

namespace Code.Infrastructure.Generator.Services
{
    public interface ILevelGeneratorService
    {
        void SetUpRootMapHolder(GameObject rootMapHolder);
        void CleanUp();
        void GenerateLevel();
    }
}