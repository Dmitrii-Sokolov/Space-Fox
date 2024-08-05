using UnityEngine;
using UnityEngine.UIElements;

public class UI : MonoBehaviour
{
    [SerializeField]
    private UIDocument UIDocument = default;

    private void Start()
    {
        foreach (var button in UIDocument.rootVisualElement.Query<Button>().ToList())
            button.clicked += () => Debug.Log(button.text);
    }
}
