using UnityEngine;

public class GoldVein : MonoBehaviour
{
    private static readonly int IsBeingMinedHash = Animator.StringToHash("IsBeingMined");

    [Header("Mining Point")]
    [SerializeField] private Transform minePoint;

    [Header("Mining Slots")]
    [SerializeField, Min(1)] private int slotCount = 5;
    [SerializeField] private float slotSpacing = 0.25f;
    [SerializeField] private Vector3 slotOffset = Vector3.zero;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool[] reservedSlots;
    private int activeMiners;

    private void Awake()
    {
        reservedSlots = new bool[Mathf.Max(1, slotCount)];

        if (minePoint == null)
        {
            Debug.LogWarning($"{name}: GoldVein has no MinePoint assigned. Falling back to transform position.");
        }
    }

    public bool TryReserveSlot(out int slotIndex)
    {
        for (int i = 0; i < reservedSlots.Length; i++)
        {
            if (!reservedSlots[i])
            {
                reservedSlots[i] = true;
                slotIndex = i;
                return true;
            }
        }

        slotIndex = -1;
        return false;
    }

    public void ReleaseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= reservedSlots.Length) return;

        reservedSlots[slotIndex] = false;
    }

    public Vector3 GetSlotPosition(int slotIndex)
    {
        int validSlotCount = Mathf.Max(1, slotCount);
        slotIndex = Mathf.Clamp(slotIndex, 0, validSlotCount - 1);

        Vector3 basePosition = minePoint != null
            ? minePoint.position
            : transform.position;

        if (validSlotCount <= 1)
        {
            return basePosition + slotOffset;
        }

        float totalWidth = (validSlotCount - 1) * slotSpacing;
        float startX = -totalWidth * 0.5f;
        float xOffset = startX + slotIndex * slotSpacing;

        return basePosition + slotOffset + new Vector3(xOffset, 0f, 0f);
    }

    public void EnterMining()
    {
        activeMiners++;
        UpdateMiningAnimation();
    }

    public void ExitMining()
    {
        activeMiners = Mathf.Max(0, activeMiners - 1);
        UpdateMiningAnimation();
    }

    private void UpdateMiningAnimation()
    {
        if (animator != null)
        {
            animator.SetBool(IsBeingMinedHash, activeMiners > 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        for (int i = 0; i < Mathf.Max(1, slotCount); i++)
        {
            Gizmos.DrawWireSphere(GetSlotPosition(i), 0.05f);
        }
    }
}
