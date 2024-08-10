using UnityEngine.AddressableAssets;

namespace SpaceFox
{
    public interface ISceneLoadSystem
    {
        IReadOnlyObservableValue<AssetReference> Scene { get; }
        IReadOnlyObservableValue<SceneLoadState> State { get; }

        void LoadScene(AssetReference scene);
    }
}
