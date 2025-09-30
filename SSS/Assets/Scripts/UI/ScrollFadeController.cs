using UnityEngine.UI;
using UnityEngine;

public class ScrollFadeController : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Material fadeMaterial;

    void Update()
    {
        if (scrollRect != null && fadeMaterial != null)
        {
            // Прогресс скролла от 0 до 1
            float scrollProgress = 1 - scrollRect.verticalNormalizedPosition;
            fadeMaterial.SetFloat("_FadeProgress", scrollProgress);
        }
    }
}