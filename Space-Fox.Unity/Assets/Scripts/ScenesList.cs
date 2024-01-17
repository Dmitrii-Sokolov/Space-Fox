using System.Collections.Generic;
using UnityEngine.AddressableAssets;

namespace SpaceFox
{
    public class ScenesList
    {
        public AssetReference MainScene { get; }

        public IReadOnlyList<AssetReference> Scenes { get; }

        public ScenesList(AssetReference mainScene)
        {
            MainScene = mainScene;
            Scenes = new List<AssetReference>() { mainScene };
        }
    }
}
