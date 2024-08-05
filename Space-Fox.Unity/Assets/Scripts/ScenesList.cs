using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;

namespace SpaceFox
{
    public class ScenesList
    {
        public AssetReference MainScene { get; }

        public IReadOnlyList<AssetReference> Scenes { get; }

        public ScenesList(AssetReference mainScene, params AssetReference[] otherScenes)
        {
            MainScene = mainScene;
            var scenes = otherScenes.ToList();
            scenes.Add(mainScene);
            Scenes = scenes;
        }
    }
}
