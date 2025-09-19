using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    private enum BindMode { ExplicitTarget, LocalPlayer, Opponent }
    [SerializeField] private BindMode bindMode = BindMode.ExplicitTarget;
    private DummyPlayerHealth_UI target;

    public Image fillImage;
    public bool smooth = true;
    public float speed = 4f;

    float targetFill = 1f;
    DummyPlayerHealth_UI bound;
    Coroutine waitCo;

    void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        TryBindOrQueue();
    }

    void OnDisable()
    {
        if (waitCo != null) { StopCoroutine(waitCo); waitCo = null; }
        Unbind();
    }

    void Update()
    {
        if (smooth && fillImage)
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFill,
                                                     Time.unscaledDeltaTime * speed);
    }

    void OnHealthChanged(float cur, float max) // 체력 변경
    {
        targetFill = (max > 0f) ? cur / max : 0f;
        if (!smooth && fillImage) fillImage.fillAmount = targetFill;
    }

    void Unbind() // 구독 대상이 존재할 경우 구독 제거
    {
        if (bound != null)
        {
            bound.onHealthChanged -= OnHealthChanged;
            bound = null;
        }
    }

    void Bind(DummyPlayerHealth_UI hp)
    {
        if (!hp) return;
        if (bound == hp) return;
        Unbind();

        // 초기값 반영 (현재 체력/최대체력으로 게이지 맞춤)
        bound = hp;
        bound.onHealthChanged += OnHealthChanged;

        // 체력 변경
        OnHealthChanged(bound.currentHP, bound.maxHP);
    }

    // UI와 캐릭터 연결
    // 캐릭터가 Instatiacte, DontDestoryOnLoad로 생성되어서 잠시 대기 기능도 추가
    void TryBindOrQueue()
    {
        if (TryBindNow()) return;
        if (waitCo != null) StopCoroutine(waitCo);
        waitCo = StartCoroutine(CoWaitAndBind());
    }

    bool TryBindNow()
    {
        // 로컬 플레이어
        var localRoot = PlayerLocator.GetLocalPlayer(); // 씬 넘어가도 유지되는 내 플레이어
        if (bindMode == BindMode.LocalPlayer)
        {
            var hpLocal = localRoot ? localRoot.GetComponent<DummyPlayerHealth_UI>() : null;
            if (hpLocal) { Bind(hpLocal); return true; }
            return false;
        }

        // 상대 플레이어
        if (bindMode == BindMode.Opponent)
        {
            // 씬에 존재하는 모든 HP 컴포넌트 중에서 "로컬이 아닌" 쪽을 선택
            var all = Object.FindObjectsByType<DummyPlayerHealth_UI>(FindObjectsInactive.Include,
              FindObjectsSortMode.None);
            var myHp = localRoot ? localRoot.GetComponent<DummyPlayerHealth_UI>() : null;
            var opp = all.FirstOrDefault(hp => hp != null && hp != myHp);
            if (opp) { Bind(opp); return true; }
            return false;
        }

        return false;
    }

    IEnumerator CoWaitAndBind() // 캐릭터 씬에 로드되기 전까지 대기
    {
        while (!TryBindNow())
            yield return null;
        waitCo = null;
    }
}
