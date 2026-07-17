using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public sealed class TitleMenuBackgroundActor : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float pixelsPerSecond = 90f;
    [SerializeField, Min(1f)] private float framesPerSecond = 10f;
    [SerializeField] private bool moveRight = true;
    [SerializeField] private float edgePadding = 80f;

    private RectTransform rectTransform;
    private Image image;
    private float animationTime;

    public void Configure(Sprite[] animationFrames, float speed, float frameRate, bool travelsRight, float padding)
    {
        frames = animationFrames;
        pixelsPerSecond = speed;
        framesPerSecond = frameRate;
        moveRight = travelsRight;
        edgePadding = padding;
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        image = GetComponent<Image>();
        image.raycastTarget = false;
        image.preserveAspect = true;
        if (frames != null && frames.Length > 0)
            image.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0)
            return;

        animationTime += Time.unscaledDeltaTime;
        image.sprite = frames[Mathf.FloorToInt(animationTime * framesPerSecond) % frames.Length];

        float direction = moveRight ? 1f : -1f;
        rectTransform.anchoredPosition += Vector2.right * (direction * pixelsPerSecond * Time.unscaledDeltaTime);

        RectTransform parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null)
            return;

        float boundary = parentRect.rect.width * 0.5f + edgePadding;
        if (moveRight && rectTransform.anchoredPosition.x > boundary)
            rectTransform.anchoredPosition = new Vector2(-boundary, rectTransform.anchoredPosition.y);
        else if (!moveRight && rectTransform.anchoredPosition.x < -boundary)
            rectTransform.anchoredPosition = new Vector2(boundary, rectTransform.anchoredPosition.y);
    }
}
