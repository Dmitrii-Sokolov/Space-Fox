using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class LoadingScreen : DisposableMonoBehaviour
    {
        [Inject] private readonly SceneLoadSystem SceneLoadSystem = default;

        [SerializeField] private SimpleProgressBar SimpleProgressBar = default;

        protected override void AwakeBeforeDestroy()
        {
            base.AwakeBeforeDestroy();

            SceneLoadSystem.State.Subscribe(OnSceneLoading).While(this);
        }

        private void OnSceneLoading(SceneLoadSystem.SceneLoadingState state)
        {
            gameObject.SetActive(!state.IsLoaded);
            SimpleProgressBar.SetProgressValue(state.Progress);
        }
    }
}
