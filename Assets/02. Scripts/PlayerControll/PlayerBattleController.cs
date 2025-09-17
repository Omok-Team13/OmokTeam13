using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerBattleController : MonoBehaviour
{
    [Tooltip("Animator의 트리거 이름")]
    public string punchTrigger = "Punch";
    public string blockTrigger = "Block";
    public string kickTrigger = "Kick";
    public string danceTrigger = "Dance";
    

    [Tooltip("Punch 애니메이션이 있는 레이어 인덱스 (보통 0)")]
    public int animatorLayer = 0;

    // 공격 중인지 외부에서 확인 가능하게
    public bool IsPunching { get; private set; }
    public bool IsBlocking { get; private set; }
    public bool IsKicking { get; private set; }
    public bool IsDancing { get; private set; }

    private Animator anim;

    // 선택: 이동 스크립트(예: CharacterMover)에 이 플래그를 전달하려면 참조를 넣어두세요.
    // public CharacterMover characterMover;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!IsPunching && (Input.GetKeyDown(KeyCode.Q)))
        {
            Debug.Log("펀치");
            StartCoroutine(DoPunch());
        }
        if (!IsBlocking && (Input.GetKeyDown(KeyCode.W)))
        {
            Debug.Log("펀치");
            StartCoroutine(DoBlock());
        }
        if (!IsKicking && (Input.GetKeyDown(KeyCode.E)))
        {
            Debug.Log("킥");
            StartCoroutine(DoKick());
        }
        if (!IsDancing && (Input.GetKeyDown(KeyCode.T)))
        {
            Debug.Log("춤");
            StartCoroutine(DoDancing());
        }
    }

    private IEnumerator DoPunch()
    {
        // 1) 상태 설정
        IsPunching = true;

        // 2) 이동 스크립트 비활성화하거나 플래그 전달(선택)
        // if (characterMover != null) characterMover.SetCanMove(false);

        // 3) 트리거로 애니메이션 재생
        anim.ResetTrigger(punchTrigger);
        anim.SetTrigger(punchTrigger);

        // 4) 애니메이션이 끝날 때까지 대기 방법 1: 상태 정보 체크
        //    (애니메이션 전환이 완료될 때까지 layer의 currentState를 체크)
        // 안전하게 대기하기 위해 루프 사용
        float timeout = 2.0f; // 혹시 무한루프 방지 타임아웃
        while (timeout > 0f)
        {
            // 현재 재생중인 상태
            var state = anim.GetCurrentAnimatorStateInfo(animatorLayer);

            // "Punch" 애니메이션이 레이어에서 재생 중인지 확인
            // 상태 이름은 Animator의 State 이름(대소문자 구분 없음)에 따라 달라질 수 있음.
            // 여기서는 루프를 빠져나오기 위해 normalizedTime >= 1.0f 인지 체크
            if (state.IsName("Punch") && state.normalizedTime >= 1.0f)
            {
                break;
            }

            // 또는 상태가 Punch로 바뀌지 않았더라도 기본상태로 돌아왔다면 끝
            // (state.IsName("Idle") 등으로 체크 가능)

            timeout -= Time.deltaTime;
            yield return null;
        }

        // 5) 공격 끝났을 때
        IsPunching = false;
        // if (characterMover != null) characterMover.SetCanMove(true);

        yield break;
    }
    private IEnumerator DoBlock()
    {
        // 1) 상태 설정
        IsBlocking = true;

        // 2) 이동 스크립트 비활성화하거나 플래그 전달(선택)
        // if (characterMover != null) characterMover.SetCanMove(false);

        // 3) 트리거로 애니메이션 재생
        anim.ResetTrigger(blockTrigger);
        anim.SetTrigger(blockTrigger);

        // 4) 애니메이션이 끝날 때까지 대기 방법 1: 상태 정보 체크
        //    (애니메이션 전환이 완료될 때까지 layer의 currentState를 체크)
        // 안전하게 대기하기 위해 루프 사용
        float timeout = 2.0f; // 혹시 무한루프 방지 타임아웃
        while (timeout > 0f)
        {
            // 현재 재생중인 상태
            var state = anim.GetCurrentAnimatorStateInfo(animatorLayer);

            // "Block" 애니메이션이 레이어에서 재생 중인지 확인
            // 상태 이름은 Animator의 State 이름(대소문자 구분 없음)에 따라 달라질 수 있음.
            // 여기서는 루프를 빠져나오기 위해 normalizedTime >= 1.0f 인지 체크
            if (state.IsName("Block") && state.normalizedTime >= 1.0f)
            {
                break;
            }

            // 또는 상태가 Punch로 바뀌지 않았더라도 기본상태로 돌아왔다면 끝
            // (state.IsName("Idle") 등으로 체크 가능)

            timeout -= Time.deltaTime;
            yield return null;
        }

        // 5) 공격 끝났을 때
        IsBlocking = false;
        // if (characterMover != null) characterMover.SetCanMove(true);

        yield break;
    }
    private IEnumerator DoKick()
    {
        // 1) 상태 설정
        IsKicking = true;

        // 2) 이동 스크립트 비활성화하거나 플래그 전달(선택)
        // if (characterMover != null) characterMover.SetCanMove(false);

        // 3) 트리거로 애니메이션 재생
        anim.ResetTrigger(kickTrigger);
        anim.SetTrigger(kickTrigger);

        // 4) 애니메이션이 끝날 때까지 대기 방법 1: 상태 정보 체크
        //    (애니메이션 전환이 완료될 때까지 layer의 currentState를 체크)
        // 안전하게 대기하기 위해 루프 사용
        float timeout = 2.0f; // 혹시 무한루프 방지 타임아웃
        while (timeout > 0f)
        {
            // 현재 재생중인 상태
            var state = anim.GetCurrentAnimatorStateInfo(animatorLayer);

            // "Punch" 애니메이션이 레이어에서 재생 중인지 확인
            // 상태 이름은 Animator의 State 이름(대소문자 구분 없음)에 따라 달라질 수 있음.
            // 여기서는 루프를 빠져나오기 위해 normalizedTime >= 1.0f 인지 체크
            if (state.IsName("Kick") && state.normalizedTime >= 1.0f)
            {
                break;
            }

            // 또는 상태가 Punch로 바뀌지 않았더라도 기본상태로 돌아왔다면 끝
            // (state.IsName("Idle") 등으로 체크 가능)

            timeout -= Time.deltaTime;
            yield return null;
        }

        // 5) 공격 끝났을 때
        IsKicking = false;
        // if (characterMover != null) characterMover.SetCanMove(true);

        yield break;
    }
    private IEnumerator DoDancing()
    {
        // 1) 상태 설정
        IsDancing = true;

        // 2) 이동 스크립트 비활성화하거나 플래그 전달(선택)
        // if (characterMover != null) characterMover.SetCanMove(false);

        // 3) 트리거로 애니메이션 재생
        anim.ResetTrigger(danceTrigger);
        anim.SetTrigger(danceTrigger);

        // 4) 애니메이션이 끝날 때까지 대기 방법 1: 상태 정보 체크
        //    (애니메이션 전환이 완료될 때까지 layer의 currentState를 체크)
        // 안전하게 대기하기 위해 루프 사용
        float timeout = 2.0f; // 혹시 무한루프 방지 타임아웃
        while (timeout > 0f)
        {
            // 현재 재생중인 상태
            var state = anim.GetCurrentAnimatorStateInfo(animatorLayer);

            // "Dance" 애니메이션이 레이어에서 재생 중인지 확인
            // 상태 이름은 Animator의 State 이름(대소문자 구분 없음)에 따라 달라질 수 있음.
            // 여기서는 루프를 빠져나오기 위해 normalizedTime >= 1.0f 인지 체크
            if (state.IsName("Dance") && state.normalizedTime >= 1.0f)
            {
                break;
            }

            // 또는 상태가 Punch로 바뀌지 않았더라도 기본상태로 돌아왔다면 끝
            // (state.IsName("Idle") 등으로 체크 가능)

            timeout -= Time.deltaTime;
            yield return null;
        }

        // 5) 공격 끝났을 때
        IsDancing = false;
        // if (characterMover != null) characterMover.SetCanMove(true);

        yield break;
    }

    // 애니메이션 이벤트로 연결할 수 있는 함수
    // AnimationClip 타임라인에 이벤트로 "OnPunchHit"를 넣으면 여기가 호출됩니다.
    public void OnPunchHit()
    {
        // 이 시점에 실제 히트 판정/데미지 적용
        Debug.Log("Punch hit! 여기서 데미지 계산/충돌 처리를 하세요.");

        // 예: Raycast or OverlapSphere로 적 체크 및 데미지 적용
    }
}
