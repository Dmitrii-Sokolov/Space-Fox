using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceFox
{
    [Serializable]
    public class ScenesList
    {
        [SerializeField] private AssetReference[] AllScenes = default;

        public AssetReference Main => AllScenes.First();
        public IReadOnlyList<AssetReference> Scenes => AllScenes;
    }
}
