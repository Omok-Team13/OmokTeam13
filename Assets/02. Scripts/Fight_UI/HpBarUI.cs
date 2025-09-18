// HpBarUI.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    public DummyPlayerHealth_UI target;
    public Image fillImage; // Hp bar 게이지
    public bool smooth = true;
    public float speed = 4f; // 부드럽게 줄어드는 속도

    float targetFill = 1f;
    DummyPlayerHealth_UI bound;
    Coroutine waitCo;

    void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        TryBindNow(); // 지금 잡히면 즉시 바인딩
        if (!bound) waitCo = StartCoroutine(CoWaitAndBind()); // 안 잡히면 다음 프레임들에서 대기
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
        bound = hp;
        bound.onHealthChanged += OnHealthChanged;
        OnHealthChanged(bound.currentHP, bound.maxHP); // 즉시 UI 초기화
    }

    bool TryBindNow()
    {
        if (target) { Bind(target); return true; }

        // DDOL 플레이어를 전역에서 가져오기
        var local = PlayerLocator.GetLocalPlayer();
        if (local)
        {
            var hp = local.GetComponent<DummyPlayerHealth_UI>();
            if (hp) { Bind(hp); return true; }
        }
        return false;
    }

    IEnumerator CoWaitAndBind()
    {
        while (!TryBindNow())
            yield return null; // 플레이어/컴포넌트가 생성될 때까지 한 프레임씩 대기
        waitCo = null;
    }
}
