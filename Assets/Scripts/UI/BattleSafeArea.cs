using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public sealed class BattleSafeArea : MonoBehaviour
{
    private Rect lastSafeArea;
    private Vector2Int lastScreenSize = new(-1, -1);

    private void OnEnable()
    {
        ApplyFullScreenIfInvalid();
        ApplySafeArea();
    }

    private void Update()
    {
        // Safe-area handling is only useful while the game is running.
        // Avoid writing RectTransform values during editor/domain reloads.
        if (!Application.isPlaying)
        {
            ApplyFullScreenIfInvalid();
            return;
        }

        if (lastSafeArea != Screen.safeArea ||
            lastScreenSize.x != Screen.width ||
            lastScreenSize.y != Screen.height)
        {
            ApplySafeArea();
        }
    }

    private void ApplySafeArea()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        Rect safe = Screen.safeArea;
        if (!IsFinite(safe) || safe.width <= 0f || safe.height <= 0f)
            safe = new Rect(0f, 0f, Screen.width, Screen.height);

        Vector2 min = new(safe.xMin / Screen.width, safe.yMin / Screen.height);
        Vector2 max = new(safe.xMax / Screen.width, safe.yMax / Screen.height);

        if (!IsFinite(min) || !IsFinite(max))
            return;

        RectTransform rect = (RectTransform)transform;
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        lastSafeArea = safe;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
    }

    private void ApplyFullScreenIfInvalid()
    {
        RectTransform rect = (RectTransform)transform;
        if (IsFinite(rect.anchorMin) && IsFinite(rect.anchorMax) &&
            IsFinite(rect.anchoredPosition) && IsFinite(rect.sizeDelta) &&
            IsFinite(rect.pivot) && IsFinite(rect.offsetMin) && IsFinite(rect.offsetMax))
        {
            return;
        }

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static bool IsFinite(Rect value) =>
        IsFinite(value.position) && IsFinite(value.size);

    private static bool IsFinite(Vector2 value) =>
        float.IsFinite(value.x) && float.IsFinite(value.y);
}
