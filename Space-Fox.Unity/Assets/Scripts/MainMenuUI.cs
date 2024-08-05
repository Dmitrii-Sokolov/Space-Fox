using SpaceFox;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class UI : MonoBehaviour
{
    [Inject] private readonly SceneLoadSystem SceneLoadSystem = default;
    [Inject] private readonly ScenesList ScenesList = default;

    [SerializeField] private UIDocument UIDocument = default;
    [SerializeField] private string MenuContainer = default;
    [SerializeField] private StyleSheet StyleSheet = default;

    private void Start()
    {
        var root = UIDocument.rootVisualElement;
        var menu = root.Q<VisualElement>(MenuContainer);

        //TODO Scene names
        for (var i = 0; i < ScenesList.Scenes.Count; i++)
        {
            var scene = ScenesList.Scenes[i];
            var button = new Button();
            button.text = scene.AssetGUID;
            button.clicked += () => SceneLoadSystem.LoadScene(scene);
            button.styleSheets.Add(StyleSheet);
            menu.Add(button);
        }
    }
}
