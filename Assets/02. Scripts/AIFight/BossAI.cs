using UnityEngine;

// 이 스크립트는 보스 캐릭터의 메인 GameObject에 붙입니다.
public class BossAI : MonoBehaviour
{
    // --- 인스펙터에서 설정할 변수들 ---
    [Header("Health Settings")]
    [Tooltip("보스의 최대 체력")]
    public float maxHealth = 200f;
    [Tooltip("보스의 현재 체력")]
    private float currentHealth;
    private bool isDead = false;

    [Header("AI Settings")]
    [Tooltip("보스의 이동 속도")]
    public float moveSpeed = 4.0f;
    [Tooltip("플레이어를 향해 회전하는 속도")]
    public float rotationSpeed = 5.0f;
    [Tooltip("다음 공격까지의 최소 대기 시간")]
    public float attackCooldown = 1.0f;

    [Header("Attack Ranges")]
    [Tooltip("이 거리 안으로 들어오면 펀치 공격 및 다채로운 스텝을 시도합니다.")]
    public float punchRange = 2.25f;
    [Tooltip("이 거리 안으로 들어오면 킥 공격을 시도합니다.")]
    public float kickRange = 3.5f;

    [Header("Dynamic Movement")]
    [Tooltip("새로운 움직임 패턴을 결정하기까지의 시간 (초)")]
    public float movePatternChangeInterval = 1.0f;
    [Tooltip("공격 쿨타임이 끝났을 때, 실제 공격을 할 확률")]
    [Range(0f, 1f)]
    public float attackProbability = 0.8f;
    [Tooltip("AI가 행동 모드를 바꾸기 전의 여유 거리")]
    public float rangeBuffer = 1.0f;

    [Header("Attack ID Settings")]
    [Tooltip("PunchMachine에 설정된 펀치 종류의 개수")]
    public int numberOfPunches = 6;
    [Tooltip("KickMachine에 설정된 킥 종류의 개수")]
    public int numberOfKicks = 3;
    [Tooltip("KickMachine에서 사용하는 AttackID의 시작 번호")]
    public int kickAttackIdStart = 10;

    [Header("Hitbox References")]
    [Tooltip("오른손 히트박스의 BossHitbox 스크립트")]
    public BossHitbox rightHandHitbox;
    [Tooltip("왼손 히트박스의 BossHitbox 스크립트")]
    public BossHitbox leftHandHitbox;
    [Tooltip("오른발 히트박스의 BossHitbox 스크립트")]
    public BossHitbox rightFootHitbox;
    [Tooltip("왼발 히트박스의 BossHitbox 스크립트")]
    public BossHitbox leftFootHitbox;

    // --- AI의 현재 상태를 정의하는 열거형(enum) ---
    private enum AIState { Chasing, Kicking, Brawling }
    private AIState currentState = AIState.Chasing;

    // --- 내부에서 사용할 변수들 ---
    private Animator animator;
    private Transform player;
    private float lastAttackTime = 0f;
    private float movePatternTimer = 0f;
    private Vector2 currentMoveVector = Vector2.zero;
    private bool isPerformingStep = false;

    void Start()
    {
        currentHealth = maxHealth;
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

        OffHitbox("All");
    }

    void Update()
    {
        if (player == null || isDead || IsInHitState())
        {
            return;
        }

        // 공격 중일 때는 회전과 상태 결정 로직만 실행하고, 이동/공격 결정은 하지 않습니다.
        bool isAttacking = IsInAttackState();
        if (isAttacking)
        {
            RotateTowardsPlayer(); // 공격 중에도 플레이어를 향하도록 부드럽게 회전
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        RotateTowardsPlayer();

        // --- 1. 상태 결정 ---
        if (currentState == AIState.Brawling && !isPerformingStep)
        {
            if (distanceToPlayer > punchRange + rangeBuffer)
            {
                currentState = AIState.Kicking;
            }
        }
        else if (currentState != AIState.Brawling)
        {
            if (distanceToPlayer <= punchRange)
            {
                currentState = AIState.Brawling;
            }
            else if (distanceToPlayer <= kickRange)
            {
                currentState = AIState.Kicking;
            }
            else
            {
                currentState = AIState.Chasing;
            }
        }

        // --- 2. 상태에 따른 행동 실행 ---
        switch (currentState)
        {
            case AIState.Chasing:
                MoveForward();
                break;

            case AIState.Kicking:
                StopMovementAndIdle();
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformAttack("Kick");
                }
                break;

            case AIState.Brawling:
                if (isPerformingStep)
                {
                    HandleDynamicMovement(distanceToPlayer);
                }
                else if (Time.time >= lastAttackTime + attackCooldown)
                {
                    if (Random.value < attackProbability)
                    {
                        PerformAttack("Punch");
                    }
                    else
                    {
                        isPerformingStep = true;
                        movePatternTimer = movePatternChangeInterval;
                        HandleDynamicMovement(distanceToPlayer);
                    }
                }
                else
                {
                    StopMovementAndIdle();
                }
                break;
        }
    }

    // --- 피격 및 사망 관련 함수들 ---
    public void TakeDamage(float damage, string hitLocation)
    {
        if (isDead)
        {
            return;
        }

        currentHealth -= damage;
        lastAttackTime = Time.time;

        Debug.Log("보스가 " + hitLocation + "에 피해를 입었습니다! 남은 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            switch (hitLocation)
            {
                case "Head": animator.SetInteger("HitLocation", 0); break;
                case "Body": animator.SetInteger("HitLocation", 1); break;
                case "Leg": animator.SetInteger("HitLocation", 2); break;
            }
            animator.SetTrigger("TakeDamage");
        }
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("보스가 쓰러졌습니다.");
        animator.SetTrigger("Die");
        this.enabled = false;
    }

    // --- 애니메이션 이벤트 함수들 ---
    public void OnHitbox(string hitboxName)
    {
        switch (hitboxName)
        {
            case "RightHand": if (rightHandHitbox != null) rightHandHitbox.Activate(); break;
            case "LeftHand": if (leftHandHitbox != null) leftHandHitbox.Activate(); break;
            case "RightFoot": if (rightFootHitbox != null) rightFootHitbox.Activate(); break;
            case "LeftFoot": if (leftFootHitbox != null) leftFootHitbox.Activate(); break;
        }
    }

    public void OffHitbox(string hitboxName)
    {
        switch (hitboxName)
        {
            case "RightHand": if (rightHandHitbox != null) rightHandHitbox.Deactivate(); break;
            case "LeftHand": if (leftHandHitbox != null) leftHandHitbox.Deactivate(); break;
            case "RightFoot": if (rightFootHitbox != null) rightFootHitbox.Deactivate(); break;
            case "LeftFoot": if (leftFootHitbox != null) leftFootHitbox.Deactivate(); break;
            case "All":
                if (rightHandHitbox != null) rightHandHitbox.Deactivate();
                if (leftHandHitbox != null) leftHandHitbox.Deactivate();
                if (rightFootHitbox != null) rightFootHitbox.Deactivate();
                if (leftFootHitbox != null) leftFootHitbox.Deactivate();
                break;
        }
    }

    // --- 나머지 행동 함수들 ---
    private void HandleDynamicMovement(float currentDistance)
    {
        movePatternTimer -= Time.deltaTime;
        if (movePatternTimer <= 0f)
        {
            isPerformingStep = false;
            StopMovementAndIdle();
            return;
        }

        if (currentMoveVector == Vector2.zero)
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
        isPerformingStep = false;
        animator.SetBool("IsMoving", true);
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveZ", 1.0f);
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void StopMovementAndIdle()
    {
        isPerformingStep = false;
        currentMoveVector = Vector2.zero;
        animator.SetBool("IsMoving", false);
        animator.SetFloat("MoveX", 0f);
        animator.SetFloat("MoveZ", 0f);
    }

    private void PerformAttack(string type)
    {
        lastAttackTime = Time.time;
        StopMovementAndIdle();

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

    private bool IsInHitState()
    {
        // 피격 애니메이션에도 "Hit" 태그를 붙여주어야 합니다.
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Hit");
    }
}

