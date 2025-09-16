using UnityEngine;

public class BossAI : MonoBehaviour
{
    // --- 인스펙터에서 설정할 변수들 ---
    [Header("AI Settings")]
    [Tooltip("보스의 이동 속도")]
    public float moveSpeed = 1.5f;
    [Tooltip("플레이어를 향해 회전하는 속도")]
    public float rotationSpeed = 5.0f;
    [Tooltip("다음 공격까지의 최소 대기 시간")]
    public float attackCooldown = 0f;

    [Header("Attack Ranges")]
    [Tooltip("이 거리 안으로 들어오면 펀치 공격을 시도합니다.")]
    public float punchRange = 2f;
    [Tooltip("이 거리 안으로 들어오면 킥 공격을 시도합니다.")]
    public float kickRange = 3.5f;

    [Header("Dynamic Movement")]
    [Tooltip("새로운 움직임 패턴을 결정하기까지의 시간 (초)")]
    public float movePatternChangeInterval = 2.0f;
    // [새로 추가] 공격할 확률 (0.7 = 70%)
    [Tooltip("공격 쿨타임이 끝났을 때, 실제 공격을 할 확률")]
    [Range(0f, 1f)]
    public float attackProbability = 0.7f;

    [Header("Attack ID Settings")]
    [Tooltip("PunchMachine에 설정된 펀치 종류의 개수")]
    public int numberOfPunches = 6;
    [Tooltip("KickMachine에 설정된 킥 종류의 개수")]
    public int numberOfKicks = 3;
    [Tooltip("KickMachine에서 사용하는 AttackID의 시작 번호")]
    public int kickAttackIdStart = 10;

    // --- 내부에서 사용할 변수들 ---
    private Animator animator;
    private Transform player;
    private float lastAttackTime = 0f;

    private float movePatternTimer = 0f;
    private Vector2 currentMoveVector = Vector2.zero;

    void Start()
    {
        animator = GetComponent<Animator>();
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("플레이어를 찾을 수 없습니다! 'Player' 태그가 설정되었는지 확인하세요.");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool isAttacking = IsInAttackState();

        if (!isAttacking)
        {
            RotateTowardsPlayer();

            if (distanceToPlayer > kickRange)
            {
                StopDynamicMovement();
                MoveForward();
            }
            else
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    // [로직 수정] 쿨타임이 끝나도 바로 공격하지 않고, 확률에 따라 결정합니다.
                    if (Random.value < attackProbability) // Random.value는 0.0 ~ 1.0 사이의 랜덤 값
                    {
                        // [선택 1] 공격을 실행 (70% 확률)
                        StopDynamicMovement();
                        if (distanceToPlayer <= punchRange)
                        {
                            PerformAttack("Punch");
                        }
                        else
                        {
                            PerformAttack("Kick");
                        }
                    }
                    else
                    {
                        // [선택 2] 공격 대신 한 번 더 스텝을 밟음 (30% 확률)
                        HandleDynamicMovement(distanceToPlayer);
                    }
                }
                else
                {
                    // 쿨타임 중이면 항상 스텝을 밟습니다.
                    HandleDynamicMovement(distanceToPlayer);
                }
            }
        }
    }

    private void HandleDynamicMovement(float currentDistance)
    {
        movePatternTimer -= Time.deltaTime;
        if (movePatternTimer <= 0f)
        {
            if (currentDistance < punchRange * 0.8f)
            {
                currentMoveVector = new Vector2(0, -1);
            }
            else
            {
                int pattern = Random.Range(0, 3);
                switch (pattern)
                {
                    case 0: currentMoveVector = new Vector2(-1, 0); break;
                    case 1: currentMoveVector = new Vector2(1, 0); break;
                    case 2: currentMoveVector = new Vector2(0, -1); break;
                }
            }
            movePatternTimer = movePatternChangeInterval;
        }

        Vector3 movement = (transform.forward * currentMoveVector.y + transform.right * currentMoveVector.x);
        transform.position += movement * moveSpeed * Time.deltaTime;

        animator.SetBool("IsMoving", true);
        animator.SetFloat("MoveX", currentMoveVector.x);
        animator.SetFloat("MoveZ", currentMoveVector.y);
    }

    private void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void MoveForward()
    {
        animator.SetBool("IsMoving", true);
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveZ", 1.0f);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void StopDynamicMovement()
    {
        animator.SetBool("IsMoving", false);
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveZ", 0f);
        movePatternTimer = 0f;
    }

    private void PerformAttack(string type)
    {
        lastAttackTime = Time.time;
        StopDynamicMovement();

        if (type == "Punch")
        {
            int randomPunchID = Random.Range(0, numberOfPunches);
            animator.SetInteger("AttackID", randomPunchID);
            animator.SetTrigger("DoPunch");
        }
        else if (type == "Kick")
        {
            int randomKickID = Random.Range(0, numberOfKicks) + kickAttackIdStart;
            animator.SetInteger("AttackID", randomKickID);
            animator.SetTrigger("DoKick");
        }
    }

    private bool IsInAttackState()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
    }
}