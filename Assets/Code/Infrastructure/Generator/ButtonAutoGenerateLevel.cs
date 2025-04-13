using Code.Infrastructure.Generator.Services;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.Infrastructure.Generator
{
    public class ButtonAutoGenerateLevel : MonoBehaviour
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

        private void Update()
        {
            _levelGeneratorService?.Tick(Time.deltaTime);
        }
        
        private void OnDestroy()
        {
            _buttonGenerateLevel.onClick.RemoveListener(OnGenerateLevelButtonClick);
        }
        
        private void OnGenerateLevelButtonClick()
        {
            if (!_levelGeneratorService.HasEnableAutoSwitch())
            {
                _levelGeneratorService.EnableAutoSwitch(true, 5f);
                _buttonGenerateLevel.image.color = Color.cyan;
            }
            else
            {
                _levelGeneratorService.EnableAutoSwitch(false, 5f);
                _buttonGenerateLevel.image.color = Color.red;
            }
        }
    }
}