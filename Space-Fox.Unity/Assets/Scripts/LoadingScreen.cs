using UnityEngine;
using VContainer;

namespace SpaceFox
{
    public class LoadingScreen : DisposableMonoBehaviour
    {
        [Inject] private readonly ISceneLoadSystem SceneLoadSystem = default;

        [SerializeField] private SimpleProgressBar SimpleProgressBar = default;

        protected override void AwakeBeforeDestroy()
        {
            base.AwakeBeforeDestroy();

            SceneLoadSystem.State.Subscribe(OnSceneLoading).While(this);
        }

        private void OnSceneLoading(SceneLoadState state)
        {
            gameObject.SetActive(!state.IsLoaded);
            SimpleProgressBar.SetProgressValue(state.Progress);
        }
    }
}
