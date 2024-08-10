using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace SpaceFox
{
    public class SceneLoadSystem : IInitializable, ISceneLoadSystem
    {
        private AsyncOperationHandle<SceneInstance> SceneLoadHandle;

        [Inject] private readonly ScenesList ScenesList = default;
        [Inject] private readonly IUpdateProxy UpdateProxy = default;

        private readonly ObservableValue<SceneLoadState> LoadingState = new();
        private readonly ObservableValue<AssetReference> CurrentScene = new();

        public IReadOnlyObservableValue<SceneLoadState> State => LoadingState;
        public IReadOnlyObservableValue<AssetReference> Scene => CurrentScene;

        public void Initialize()
        {
#if UNITY_EDITOR
            var currentScene = SceneManager.GetActiveScene();
            CurrentScene.Value = ScenesList.Scenes.FirstOrDefault(scene => scene.editorAsset.name == currentScene.name);
            LoadingState.Value = SceneLoadState.Loaded;
#else
            LoadScene(ScenesList.Main);
#endif
        }

        public async void LoadScene(AssetReference scene)
        {
            if (scene == CurrentScene.Value)
                return;

            LoadingState.Value = SceneLoadState.Unloaded;
            CurrentScene.Value = scene;

            if (SceneLoadHandle.IsValid())
                Addressables.Release(SceneLoadHandle);

            SceneLoadHandle = Addressables.LoadSceneAsync(scene);
            var loadingStateUpdater = UpdateProxy.Update.Subscribe(
                () => LoadingState.Value = SceneLoadState.Loading(SceneLoadHandle.PercentComplete));

            await SceneLoadHandle.Task;

            loadingStateUpdater.Dispose();
            LoadingState.Value = SceneLoadState.Loaded;
        }
    }
}
