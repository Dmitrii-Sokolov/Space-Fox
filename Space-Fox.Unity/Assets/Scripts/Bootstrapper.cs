using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class Bootstrapper : MonoBehaviour
    {
        private static bool Initialized = false;

        [Inject] private readonly SceneLoadSystem SceneLoadSystem = default;

        private void Awake()
        {
            if (!Initialized)
            {
                Initialized = true;
                SceneLoadSystem.Initialize(gameObject.scene.name);
            }

            Destroy(gameObject);
        }
    }
}
