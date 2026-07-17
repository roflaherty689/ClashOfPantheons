using UnityEngine;

[DisallowMultipleComponent]
public class BasePresentation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer castleRenderer;
    [SerializeField] private SpriteRenderer handInRenderer;
    [SerializeField] private Transform handInTransform;
    [SerializeField, Min(0f)] private float inwardOffset = 0.5f;

    public Transform DropOffPoint => handInTransform;

    private void Awake()
    {
        Base battleBase = GetComponent<Base>();
        if (battleBase != null)
        {
            PositionHandIn(battleBase.Team);
        }
    }

    public void Apply(FactionData factionData, Team team)
    {
        PositionHandIn(team);

        if (castleRenderer != null)
        {
            castleRenderer.color = Color.white;
        }

        if (handInRenderer != null)
        {
            handInRenderer.color = Color.white;
        }

        if (factionData == null)
        {
            Debug.LogWarning($"{name}: Cannot apply building presentation without faction data.", this);
            return;
        }

        if (factionData.CastleSprite != null && castleRenderer != null)
        {
            castleRenderer.sprite = factionData.CastleSprite;
        }

        if (factionData.HandInSprite != null && handInRenderer != null)
        {
            handInRenderer.sprite = factionData.HandInSprite;
        }
    }

    public bool ValidateReferences()
    {
        bool isValid = castleRenderer != null && handInRenderer != null && handInTransform != null;
        if (!isValid)
        {
            Debug.LogError($"{name}: BasePresentation is missing renderer or hand-in references.", this);
        }

        return isValid;
    }

    private void PositionHandIn(Team team)
    {
        if (handInTransform == null) return;

        Vector3 localPosition = handInTransform.localPosition;
        localPosition.x = team == Team.Left ? inwardOffset : -inwardOffset;
        handInTransform.localPosition = localPosition;
    }
}
