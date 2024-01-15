using UnityEngine;

namespace SpaceFox
{
    public class SimpleProgressBar : MonoBehaviour
    {
        [SerializeField] private RectTransform mFillRect = default;

        public void SetProgressValue(float value)
            => mFillRect.anchorMax = new Vector2(Mathf.Clamp01(value), 1f);
    }
}
