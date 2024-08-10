namespace SpaceFox
{
    public readonly struct SceneLoadState
    {
        public bool IsLoaded { get; }
        public float Progress { get; }

        public static SceneLoadState Unloaded => new(false, 0f);
        public static SceneLoadState Loaded => new(true, 1f);

        public SceneLoadState(bool isLoaded, float progress) : this()
        {
            IsLoaded = isLoaded;
            Progress = progress;
        }

        public static SceneLoadState Loading(float state) => new(false, state);
    }
}
