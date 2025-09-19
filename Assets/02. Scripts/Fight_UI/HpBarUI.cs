// HpBarUI.cs (핵심만 추가/변경)
using System.Collections;
using System.Linq; // for Where/FirstOrDefault
using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    public enum BindMode { ExplicitTarget, LocalPlayer, Opponent }
    [Header("Binding")]
    public BindMode bindMode = BindMode.ExplicitTarget;
    public DummyPlayerHealth_UI target;

    [Header("UI")]
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

    void OnHealthChanged(float cur, float max)
    {
        targetFill = (max > 0f) ? cur / max : 0f;
        if (!smooth && fillImage) fillImage.fillAmount = targetFill;
    }

    void Unbind()
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
        bound = hp;
        bound.onHealthChanged += OnHealthChanged;
        OnHealthChanged(bound.currentHP, bound.maxHP);
    }

    void TryBindOrQueue()
    {
        if (TryBindNow()) return;
        if (waitCo != null) StopCoroutine(waitCo);
        waitCo = StartCoroutine(CoWaitAndBind());
    }

    bool TryBindNow()
    {
        // 1) 명시적 타깃(Inspector에 연결돼 있으면 그걸 사용)
        if (bindMode == BindMode.ExplicitTarget && target)
        {
            Bind(target);
            return true;
        }

        // 2) 로컬 플레이어
        var localRoot = PlayerLocator.GetLocalPlayer(); // 씬 넘어가도 유지되는 내 플레이어
        if (bindMode == BindMode.LocalPlayer)
        {
            var hpLocal = localRoot ? localRoot.GetComponent<DummyPlayerHealth_UI>() : null;
            if (hpLocal) { Bind(hpLocal); return true; }
            return false;
        }

        // 3) 상대 플레이어 (내가 아닌 쪽을 찾아 매핑)
        if (bindMode == BindMode.Opponent)
        {
            // 씬에 존재하는 모든 HP 컴포넌트 중에서 "로컬이 아닌" 쪽을 선택
            var all = Object.FindObjectsOfType<DummyPlayerHealth_UI>(true);
            var myHp = localRoot ? localRoot.GetComponent<DummyPlayerHealth_UI>() : null;
            var opp = all.FirstOrDefault(hp => hp != null && hp != myHp);
            if (opp) { Bind(opp); return true; }
            return false;
        }

        return false;
    }

    IEnumerator CoWaitAndBind()
    {
        while (!TryBindNow())
            yield return null;
        waitCo = null;
    }
}
