using System;
using Code.Infrastructure.Generator.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code
{
    public class ButtonGenerateLevel : MonoBehaviour
    {
        [SerializeField] private Button _buttonGenerateLevel;
        
        private ILevelGeneratorService _levelGeneratorService;

        private void OnValidate()
        {
            if(_buttonGenerateLevel == null)
                _buttonGenerateLevel = GetComponent<Button>();
        }

        [Inject]
        public void Construct(ILevelGeneratorService levelGeneratorService)
        {
            _levelGeneratorService = levelGeneratorService;
        }
        
        private void Start()
        {
            _buttonGenerateLevel.onClick.AddListener(OnGenerateLevelButtonClick);
        }

        private void OnDestroy()
        {
            _buttonGenerateLevel.onClick.RemoveListener(OnGenerateLevelButtonClick);
        }
        
        private void OnGenerateLevelButtonClick()
        {
            _levelGeneratorService.GenerateLevel();
        }
    }
}