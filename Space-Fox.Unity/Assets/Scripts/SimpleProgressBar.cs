using UnityEngine;

namespace SpaceFox
{
    public class SimpleProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform FillRect = default;

        public void SetProgressValue(float value)
            => FillRect.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}
