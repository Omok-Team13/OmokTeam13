using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CharacterModeSwitcherRuntime : MonoBehaviour
{
    [Header("찾기 설정")]
    [Tooltip("IntroCharacter에 설정된 태그(가능하면 태그 사용 권장). 비우면 name으로 찾음.")]
    public string targetTag = "Player";

    [Tooltip("태그를 사용하지 않을 때 찾을 이름(정확히 일치).")]
    public string targetName = "IntroCharacter";

    [Tooltip("찾기 재시도 간격(초)")]
    public float retryInterval = 0.2f;

    [Tooltip("최대 재시도 시간(초). 0 이하이면 무제한 재시도")]
    public float maxWaitSeconds = 3.0f;

    // 내부 캐시
    private GameObject targetGO;
    private SitEmoteController sitEmote;
    private PlayerBattleController battleController;
    private Coroutine waitingCoroutine;

    #region Public API for Buttons
    // 버튼의 OnClick에 연결해서 사용
    public void EnableSitModeButton()
    {
        RequestSwitch(Mode.Sit);
    }

    public void EnableBattleModeButton()
    {
        RequestSwitch(Mode.Battle);
    }
    #endregion

    enum Mode { Sit, Battle }

    void RequestSwitch(Mode mode)
    {
        // 이미 캐시가 있으면 즉시 처리
        if (EnsureCachedReferences())
        {
            ApplyMode(mode);
            return;
        }

        // 캐시가 없으면 재시도 코루틴 시작(중복 실행 방지)
        if (waitingCoroutine != null)
        {
            StopCoroutine(waitingCoroutine);
            waitingCoroutine = null;
        }
        waitingCoroutine = StartCoroutine(WaitForTargetAndApply(mode));
    }

    IEnumerator WaitForTargetAndApply(Mode mode)
    {
        float elapsed = 0f;
        while (true)
        {
            if (EnsureCachedReferences())
            {
                ApplyMode(mode);
                waitingCoroutine = null;
                yield break;
            }

            if (maxWaitSeconds > 0f && elapsed >= maxWaitSeconds)
            {
                Debug.LogWarning($"[CharacterModeSwitcherRuntime] 타겟을 찾지 못했습니다 (timeout {maxWaitSeconds}s).");
                waitingCoroutine = null;
                yield break;
            }

            yield return new WaitForSeconds(retryInterval);
            elapsed += retryInterval;
        }
    }

    // 타겟 및 컴포넌트 찾고 캐시한다. 찾았으면 true 반환
    bool EnsureCachedReferences()
    {
        if (targetGO != null && sitEmote != null && battleController != null)
            return true;

        // 1) 태그 기반 우선
        if (!string.IsNullOrEmpty(targetTag))
        {
            try
            {
                var go = GameObject.FindWithTag(targetTag);
                if (go != null)
                {
                    CacheFromGameObject(go);
                    return sitEmote != null || battleController != null;
                }
            }
            catch (UnityException)
            {
                // 태그가 프로젝트에 등록되지 않은 경우 예외가 발생할 수 있음.
                // 무시하고 name 기반 탐색으로 넘어감.
            }
        }

        // 2) name 기반 (GameObject.Find)
        if (!string.IsNullOrEmpty(targetName))
        {
            var goByName = GameObject.Find(targetName);
            if (goByName != null)
            {
                CacheFromGameObject(goByName);
                return sitEmote != null || battleController != null;
            }
        }

        // 3) 컴포넌트 직접 탐색(비활성 오브젝트 포함, Unity 2020+ 지원)
#if UNITY_2020_1_OR_NEWER
        var sit = FindObjectOfType<SitEmoteController>(true);
        var bat = FindObjectOfType<PlayerBattleController>(true);
        if (sit != null || bat != null)
        {
            // 우선 둘 중 하나라도 있으면 그 gameObject를 캐시 대상으로 삼는다 (우선 sit 우선)
            var go = sit != null ? sit.gameObject : bat.gameObject;
            CacheFromGameObject(go);
            return sitEmote != null || battleController != null;
        }
#endif

        return false;
    }

    void CacheFromGameObject(GameObject go)
    {
        if (go == null) return;
        targetGO = go;
        sitEmote = go.GetComponent<SitEmoteController>();
        battleController = go.GetComponent<PlayerBattleController>();
        Debug.Log($"[CharacterModeSwitcherRuntime] Cached target '{go.name}'. sit:{(sitEmote != null)} battle:{(battleController != null)}");
    }

    void ApplyMode(Mode mode)
    {
        if (targetGO == null)
        {
            Debug.LogWarning("[CharacterModeSwitcherRuntime] 타겟이 없습니다. 스위치 실패.");
            return;
        }

        switch (mode)
        {
            case Mode.Sit:
                if (sitEmote != null) sitEmote.enabled = true;
                if (battleController != null) battleController.enabled = false;
                Debug.Log("[CharacterModeSwitcherRuntime] Sit 모드 적용: SitEmote ON, Battle OFF");
                break;

            case Mode.Battle:
                if (sitEmote != null) sitEmote.enabled = false;
                if (battleController != null) battleController.enabled = true;
                Debug.Log("[CharacterModeSwitcherRuntime] Battle 모드 적용: Battle ON, SitEmote OFF");
                break;
        }
    }

    // 외부에서 강제 리셋(예: 씬 전환 시 캐시 초기화 필요하면 호출)
    public void ResetCache()
    {
        targetGO = null;
        sitEmote = null;
        battleController = null;
        if (waitingCoroutine != null)
        {
            StopCoroutine(waitingCoroutine);
            waitingCoroutine = null;
        }
        Debug.Log("[CharacterModeSwitcherRuntime] 캐시 리셋됨");
    }
}
