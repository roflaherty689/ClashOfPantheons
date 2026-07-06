using UnityEngine;

public class WorkerUnit : MonoBehaviour
{
    private enum WorkerState
    {
        MovingToMine,
        Mining,
        ReturningToDropOff
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Mining")]
    [SerializeField] private float mineDuration = 3f;
    [SerializeField] private int goldPerTrip = 5;

    [Header("Visuals")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private GameManager gameManager;

    private bool hasStopped;

    private WorkerManager manager;
    private GoldVein goldVein;
    private Transform dropOffPoint;

    private WorkerState state;
    private Vector3 targetPosition;
    private int mineSlotIndex = -1;
    private float mineTimer;
    private bool isCarrying;

    public void Initialize(WorkerManager manager, GoldVein goldVein, Transform dropOffPoint)
    {
        this.manager = manager;
        this.goldVein = goldVein;
        this.dropOffPoint = dropOffPoint;

        gameManager = FindFirstObjectByType<GameManager>();

        MoveToMine();
    }

    private void Update()
    {
        if (gameManager != null && gameManager.IsGameOver)
        {
            StopWorker();
            return;
        }

        switch (state)
        {
            case WorkerState.MovingToMine:
                MoveTowardsTarget();

                if (HasReachedTarget())
                {
                    StartMining();
                }

                break;

            case WorkerState.Mining:
                mineTimer -= Time.deltaTime;

                if (mineTimer <= 0f)
                {
                    FinishMining();
                }

                break;

            case WorkerState.ReturningToDropOff:
                MoveTowardsTarget();

                if (HasReachedTarget())
                {
                    DepositGold();
                    MoveToMine();
                }

                break;
        }

        UpdateAnimator();
    }

    private void StopWorker()
    {
        if (hasStopped) return;

        hasStopped = true;

        // Reset worker state
        state = WorkerState.ReturningToDropOff;
        isCarrying = false;

        // Release any reserved mining slot
        if (goldVein != null)
        {
            if (mineSlotIndex >= 0)
            {
                goldVein.ReleaseSlot(mineSlotIndex);
                mineSlotIndex = -1;
            }

            goldVein.SetBeingMined(false);
        }

        // Force the animator into Idle
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetBool("IsMining", false);
            animator.SetBool("IsCarrying", false);

            // Immediately evaluate the new state
            animator.Update(0f);
        }
    }

    private void MoveToMine()
    {
        isCarrying = false;
        state = WorkerState.MovingToMine;

        mineSlotIndex = goldVein.ReserveSlot();
        targetPosition = goldVein.GetSlotPosition(mineSlotIndex);
    }

    private void StartMining()
    {
        state = WorkerState.Mining;
        mineTimer = mineDuration;

        transform.position = targetPosition;
        goldVein.SetBeingMined(true);
    }

    private void FinishMining()
    {
        isCarrying = true;
        state = WorkerState.ReturningToDropOff;
        targetPosition = dropOffPoint.position;

        goldVein.ReleaseSlot(mineSlotIndex);
        mineSlotIndex = -1;
        goldVein.SetBeingMined(false);
    }

    private void DepositGold()
    {
        manager.AddGold(goldPerTrip);
        isCarrying = false;
    }

    private void MoveTowardsTarget()
    {
        Vector3 previousPosition = transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        UpdateFacing(previousPosition);
    }

    private bool HasReachedTarget()
    {
        return Vector3.Distance(transform.position, targetPosition) <= arriveDistance;
    }

    private void UpdateFacing(Vector3 previousPosition)
    {
        if (spriteRenderer == null) return;

        float xMovement = transform.position.x - previousPosition.x;

        if (Mathf.Abs(xMovement) > 0.001f)
        {
            spriteRenderer.flipX = xMovement < 0f;
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null) return;

        bool isMining = state == WorkerState.Mining;
        bool isMoving = state != WorkerState.Mining;

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsMining", isMining);
        animator.SetBool("IsCarrying", isCarrying);
    }

    private void OnDisable()
    {
        if (goldVein != null && mineSlotIndex >= 0)
        {
            goldVein.ReleaseSlot(mineSlotIndex);
        }
    }
}