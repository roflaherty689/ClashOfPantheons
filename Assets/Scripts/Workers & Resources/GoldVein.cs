using UnityEngine;

public class GoldVein : MonoBehaviour
{
    [Header("Mining Point")]
    [SerializeField] private Transform minePoint;

    [Header("Mining Slots")]
    [SerializeField] private int slotCount = 5;
    [SerializeField] private float slotSpacing = 0.25f;
    [SerializeField] private Vector3 slotOffset = Vector3.zero;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool[] reservedSlots;
    private int activeMiners;

    private void Awake()
    {
        reservedSlots = new bool[slotCount];

        if (minePoint == null)
        {
            Debug.LogWarning($"{name}: GoldVein has no MinePoint assigned. Falling back to transform position.");
        }
    }

    public int ReserveSlot()
    {
        for (int i = 0; i < reservedSlots.Length; i++)
        {
            if (!reservedSlots[i])
            {
                reservedSlots[i] = true;
                return i;
            }
        }

        return Random.Range(0, slotCount);
    }

    public void ReleaseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= reservedSlots.Length) return;

        reservedSlots[slotIndex] = false;
    }

    public Vector3 GetSlotPosition(int slotIndex)
    {
        Vector3 basePosition = minePoint != null
            ? minePoint.position
            : transform.position;

        if (slotCount <= 1)
        {
            return basePosition + slotOffset;
        }

        float totalWidth = (slotCount - 1) * slotSpacing;
        float startX = -totalWidth * 0.5f;
        float xOffset = startX + slotIndex * slotSpacing;

        return basePosition + slotOffset + new Vector3(xOffset, 0f, 0f);
    }

    public void SetBeingMined(bool beingMined)
    {
        if (beingMined)
        {
            activeMiners++;
        }
        else
        {
            activeMiners = Mathf.Max(0, activeMiners - 1);
        }

        if (animator != null)
        {
            animator.SetBool("IsBeingMined", activeMiners > 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < slotCount; i++)
        {
            Gizmos.DrawWireSphere(GetSlotPosition(i), 0.05f);
        }
    }
}