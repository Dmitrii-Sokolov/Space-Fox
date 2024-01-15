using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class SceneLoadSystemInstaller : MonoInstaller
    {
        [SerializeField] private LoadingScreen LoadingScreen = default;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SceneLoadSystem>().AsSingle();
            Container.BindInterfacesAndSelfTo<LoadingScreen>().FromComponentInNewPrefab(LoadingScreen).AsSingle().NonLazy();
        }
    }
}
