using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SpaceFox
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private LoadingScreen LoadingScreen = default;
        [SerializeField] private ScenesList ScenesList = default;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<EntryPointUpdater>(Lifetime.Singleton).As<IUpdateProxy>();

            builder.RegisterInstance(ScenesList);
            builder.RegisterEntryPoint<SceneLoadSystem>(Lifetime.Singleton).As<ISceneLoadSystem>();
            builder.RegisterComponentInNewPrefab(LoadingScreen, Lifetime.Singleton);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            ScenesList.OnValidate();
        }
#endif
    }
}
