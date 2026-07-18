using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ProductionCardView : MonoBehaviour
{
    [SerializeField] private UnitRole role;
    [SerializeField] private Graphic interactionGraphic;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image artwork;
    [SerializeField] private Image portraitPaper;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI tierText;
    [SerializeField] private TextMeshProUGUI actionText;

    public UnitRole Role => role;
    public Graphic InteractionGraphic => interactionGraphic;
    public Button PurchaseButton => purchaseButton;
    public Image Artwork => artwork;
    public Sprite PortraitPaperSprite => portraitPaper != null ? portraitPaper.sprite : null;
    public TextMeshProUGUI StatusText => statusText;
    public TextMeshProUGUI TierText => tierText;
    public TextMeshProUGUI ActionText => actionText;

    public void Configure(
        UnitRole configuredRole,
        Graphic configuredInteractionGraphic,
        Button configuredPurchaseButton,
        Image configuredArtwork,
        Image configuredPortraitPaper,
        TextMeshProUGUI configuredStatusText,
        TextMeshProUGUI configuredTierText,
        TextMeshProUGUI configuredActionText)
    {
        role = configuredRole;
        interactionGraphic = configuredInteractionGraphic;
        purchaseButton = configuredPurchaseButton;
        artwork = configuredArtwork;
        portraitPaper = configuredPortraitPaper;
        statusText = configuredStatusText;
        tierText = configuredTierText;
        actionText = configuredActionText;
    }

    public bool HasCompleteBindings =>
        interactionGraphic != null && purchaseButton != null && artwork != null &&
        portraitPaper != null && statusText != null && tierText != null && actionText != null;
}
