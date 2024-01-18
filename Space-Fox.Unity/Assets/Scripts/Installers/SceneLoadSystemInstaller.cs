using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace SpaceFox
{
    public class SceneLoadSystemInstaller : MonoInstaller
    {
        [SerializeField] private AssetReference MainScene = default;

        [SerializeField] private LoadingScreen LoadingScreen = default;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ScenesList>().FromMethod(_ => new ScenesList(MainScene)).AsSingle();
            Container.BindInterfacesAndSelfTo<SceneLoadSystem>().AsSingle();

            Container.BindInterfacesAndSelfTo<LoadingScreen>().FromComponentInNewPrefab(LoadingScreen).AsSingle().NonLazy();
        }
    }
}
