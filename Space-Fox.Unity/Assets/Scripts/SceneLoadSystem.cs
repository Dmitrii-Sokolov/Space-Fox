namespace SpaceFox
{
    public class SceneLoadSystem
    {
        public struct SceneLoadingState
        {
            public bool IsLoaded { get; set; }
            public float Progress { get; set; }
        }

        private ObservableValue<SceneLoadingState> LoadingState = new();

        public IReadOnlyObservableValue<SceneLoadingState> State => LoadingState;
    }
}
