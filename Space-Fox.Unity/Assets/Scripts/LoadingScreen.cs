using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class LoadingScreen : DisposableMonoBehaviour
    {
        [Inject] private readonly SceneLoadSystem SceneLoadSystem = default;

        [SerializeField] private SimpleProgressBar SimpleProgressBar = default;

        private void Awake()
            => SceneLoadSystem.State.Subscribe(OnSceneLoading).While(this);

        private void OnSceneLoading(SceneLoadSystem.SceneLoadingState state)
        {
            gameObject.SetActive(!state.IsLoaded);
            SimpleProgressBar.SetProgressValue(state.Progress);
        }
    }
}
