using NaughtyAttributes;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UISizeConstrainer : MonoBehaviour
{
    private RectTransform rectTransform;

    [Header("Minimum Size")]
    public bool enforceMinSize = true;
    [ShowIf("enforceMinSize")] public float minWidth = 100f;
    [ShowIf("enforceMinSize")] public float minHeight = 100f;

    [Header("Maximum Size")]
    public bool enforceMaxSize = false;
    [ShowIf("enforceMaxSize")] public float maxWidth = 1000f;
    [ShowIf("enforceMaxSize")] public float maxHeight = 1000f;

    [Header("Aspect Ratio")]
    public bool maintainAspect = false;
    [ShowIf("enforceMaxSize"), Tooltip("Width / Height")] public float aspectRatio = 1f;

    [Header("Runtime Settings")]
    public bool enforceDuringPlayMode = false;

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying || enforceDuringPlayMode)
        {
            EnforceConstraints();
        }
#else
        if (enforceDuringPlayMode)
        {
            EnforceConstraints();
        }
#endif
    }

    private void EnforceConstraints()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        Vector2 size = rectTransform.sizeDelta;
        float width = size.x;
        float height = size.y;

        // Apply minimum size constraints
        if (enforceMinSize)
        {
            width = Mathf.Max(width, minWidth);
            height = Mathf.Max(height, minHeight);
        }

        // Apply maximum size constraints
        if (enforceMaxSize)
        {
            width = Mathf.Min(width, maxWidth);
            height = Mathf.Min(height, maxHeight);
        }

        // Maintain aspect ratio
        if (maintainAspect)
        {
            float currentAspect = width / height;
            if (currentAspect > aspectRatio)
            {
                width = height * aspectRatio;
            }
            else
            {
                height = width / aspectRatio;
            }
        }

        rectTransform.sizeDelta = new Vector2(width, height);
    }
}
