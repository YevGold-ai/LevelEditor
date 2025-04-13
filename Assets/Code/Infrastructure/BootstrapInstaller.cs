using Code.Infrastructure.Factory;
using Code.Infrastructure.Generator.Factory;
using Code.Infrastructure.Generator.Services;
using Code.Infrastructure.Services.PersistenceProgress;
using Code.Infrastructure.Services.SaveLoad;
using Code.Infrastructure.Services.StaticData;
using Services.PersistenceProgress;
using UnityEngine.SceneManagement;
using Zenject;

namespace Code.Infrastructure
{
    public class BootstrapInstaller : MonoInstaller, IInitializable
    {
        private const string SceneName = "Game";
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BootstrapInstaller>().FromInstance(this).AsSingle();

            BindFactory();
            BindSaveLoad();
            BindProgressData();
            BindStaticData();
            BindGenerator();
        }

        private void BindFactory()
        {
            Container.BindInterfacesTo<UIFactory>().AsSingle();
            Container.BindInterfacesTo<TileFactory>().AsSingle();
        }
        
        private void BindSaveLoad() =>
            Container.Bind<ISaveLoadService>().To<SaveLoadService>().AsSingle();

        private void BindProgressData() =>
            Container.Bind<IPersistenceProgressService>().To<PersistenceProgressService>().AsSingle();
        
        
        private void BindStaticData()
        {
            Container.Bind<IStaticDataService>().To<StaticDataService>().AsSingle();
        }

        private void BindGenerator()
        {
            Container.Bind<ILevelGeneratorService>().To<LevelGeneratorService>().AsSingle();
        }
        
        public void Initialize()
        {
            Container.Resolve<IStaticDataService>().LoadData();
            
            SceneManager.LoadScene(SceneName);
        }
    }
}