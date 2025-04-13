using System;
using UnityEngine;

namespace Code.Infrastructure.Generator.Services
{
    public interface ILevelGeneratorService
    {
        void SetUpRootMapHolder(GameObject rootMapHolder);
        void CleanUp(Action onComplete = null);
        void GenerateLevel();
        void LoadNextLevel();
    }
}