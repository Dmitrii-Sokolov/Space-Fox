using UnityEngine;
using Zenject;

namespace SpaceFox
{
    public class Bootstrapper : MonoBehaviour
    {
        [Inject] private readonly SceneLoadSystem SceneLoadSystem = default;

        private void Awake()
        {
            SceneLoadSystem.Initialize(gameObject.scene);
            
            Destroy(gameObject);
        }
    }
}
