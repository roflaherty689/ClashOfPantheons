using UnityEngine;

public class WorkerUnit : MonoBehaviour
{
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    private static readonly int IsMiningHash = Animator.StringToHash("IsMining");
    private static readonly int IsCarryingHash = Animator.StringToHash("IsCarrying");

    private enum WorkerState
    {
        WaitingForMineSlot,
        MovingToMine,
        Mining,
        ReturningToDropOff
    }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float arriveDistance = 0.05f;

    [Header("Mining")]
    [SerializeField] private float mineDuration = 3f;

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
    private bool isMining;
    private bool isCarrying;

    public void Initialize(WorkerManager manager, GoldVein goldVein, Transform dropOffPoint)
    {
        this.manager = manager;
        this.goldVein = goldVein;
        this.dropOffPoint = dropOffPoint;

        gameManager = FindAnyObjectByType<GameManager>();

        TryMoveToMine();
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
            case WorkerState.WaitingForMineSlot:
                TryMoveToMine();
                break;

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
                    TryMoveToMine();
                }

                break;
        }

        UpdateAnimator();
    }

    private void StopWorker()
    {
        if (hasStopped) return;

        hasStopped = true;

        isCarrying = false;
        ReleaseMineAccess();

        // Force the animator into Idle
        if (animator != null)
        {
            animator.SetBool(IsMovingHash, false);
            animator.SetBool(IsMiningHash, false);
            animator.SetBool(IsCarryingHash, false);

            // Immediately evaluate the new state
            animator.Update(0f);
        }
    }

    private bool TryMoveToMine()
    {
        isCarrying = false;

        if (!goldVein.TryReserveSlot(out mineSlotIndex))
        {
            state = WorkerState.WaitingForMineSlot;
            return false;
        }

        state = WorkerState.MovingToMine;
        targetPosition = goldVein.GetSlotPosition(mineSlotIndex);
        return true;
    }

    private void StartMining()
    {
        state = WorkerState.Mining;
        mineTimer = mineDuration;
        isMining = true;

        transform.position = targetPosition;
        goldVein.EnterMining();
    }

    private void FinishMining()
    {
        isCarrying = true;
        state = WorkerState.ReturningToDropOff;
        targetPosition = dropOffPoint.position;

        ReleaseMineAccess();
    }

    private void DepositGold()
    {
        manager.DepositWorkerGold();
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

        bool isWaiting = state == WorkerState.WaitingForMineSlot;
        bool isMoving = !isWaiting && state != WorkerState.Mining;

        animator.SetBool(IsMovingHash, isMoving);
        animator.SetBool(IsMiningHash, isMining);
        animator.SetBool(IsCarryingHash, isCarrying);
    }

    private void OnDisable()
    {
        ReleaseMineAccess();

        if (!hasStopped && manager != null)
        {
            state = WorkerState.WaitingForMineSlot;
        }
    }

    private void OnDestroy()
    {
        if (manager != null)
        {
            manager.UnregisterWorker(this);
        }
    }

    private void ReleaseMineAccess()
    {
        if (goldVein == null) return;

        if (isMining)
        {
            isMining = false;
            goldVein.ExitMining();
        }

        if (mineSlotIndex >= 0)
        {
            goldVein.ReleaseSlot(mineSlotIndex);
            mineSlotIndex = -1;
        }
    }
}
