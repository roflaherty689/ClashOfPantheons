using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public sealed class ProductionCardView : MonoBehaviour
{
    [FormerlySerializedAs("role")]
    [SerializeField] private ProductionSlotId slotId;
    [SerializeField] private Graphic interactionGraphic;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Image artwork;
    [SerializeField] private Image portraitPaper;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI tierText;
    [SerializeField] private TextMeshProUGUI actionText;

    public ProductionSlotId SlotId => slotId;
    public Graphic InteractionGraphic => interactionGraphic;
    public Button PurchaseButton => purchaseButton;
    public Image Artwork => artwork;
    public Sprite PortraitPaperSprite => portraitPaper != null ? portraitPaper.sprite : null;
    public TextMeshProUGUI TitleText
    {
        get
        {
            if (titleText == null)
            {
                titleText = ResolveLegacyTitle();
            }

            return titleText;
        }
    }
    public TextMeshProUGUI StatusText => statusText;
    public TextMeshProUGUI TierText => tierText;
    public TextMeshProUGUI ActionText => actionText;

    public void Configure(
        ProductionSlotId configuredSlotId,
        Graphic configuredInteractionGraphic,
        Button configuredPurchaseButton,
        Image configuredArtwork,
        Image configuredPortraitPaper,
        TextMeshProUGUI configuredTitleText,
        TextMeshProUGUI configuredStatusText,
        TextMeshProUGUI configuredTierText,
        TextMeshProUGUI configuredActionText)
    {
        slotId = configuredSlotId;
        interactionGraphic = configuredInteractionGraphic;
        purchaseButton = configuredPurchaseButton;
        artwork = configuredArtwork;
        portraitPaper = configuredPortraitPaper;
        titleText = configuredTitleText;
        statusText = configuredStatusText;
        tierText = configuredTierText;
        actionText = configuredActionText;
    }

    public bool HasCompleteBindings =>
        interactionGraphic != null && purchaseButton != null && artwork != null &&
        portraitPaper != null && statusText != null && tierText != null && actionText != null;

    private TextMeshProUGUI ResolveLegacyTitle()
    {
        string legacyTitle = slotId switch
        {
            ProductionSlotId.Standard0 => "MELEE",
            ProductionSlotId.Standard1 => "ARCHER",
            ProductionSlotId.Standard2 => "CAVALRY",
            ProductionSlotId.Standard3 => "SIEGE",
            ProductionSlotId.Mythic => "MYTHIC",
            _ => string.Empty
        };

        for (int index = 0; index < transform.childCount; index++)
        {
            if (transform.GetChild(index).TryGetComponent(out TextMeshProUGUI candidate) &&
                candidate.text == legacyTitle)
            {
                return candidate;
            }
        }

        return null;
    }
}
