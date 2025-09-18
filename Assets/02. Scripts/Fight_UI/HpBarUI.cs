// HpBarUI.cs
using UnityEngine;
using UnityEngine.UI;

public class HpBarUI : MonoBehaviour
{
    public PlayerHealth_UItest target;
    public Image fillImage; // Hp bar 게이지
    public bool smooth = true;
    public float speed = 4f; // 부드럽게 줄어드는 속도

    float targetFill = 1f;

    void Awake()
    {
        if (!fillImage) fillImage = GetComponent<Image>();
    }

    void OnEnable()
    {
        if (!target) target = FindFirstObjectByType<PlayerHealth_UItest>();
        if (target != null)
        {
            target.onHealthChanged += OnHealthChanged;
            OnHealthChanged(target.currentHP, target.maxHP); // HP 초기값 설정
        }
    }

    void OnDisable()
    {
        if (target != null)
            target.onHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(float cur, float max)
    {
        targetFill = (max > 0f) ? cur / max : 0f;
        if (!smooth && fillImage) fillImage.fillAmount = targetFill;
    }

    void Update()
    {
        if (smooth && fillImage)
            fillImage.fillAmount = Mathf.MoveTowards(fillImage.fillAmount, targetFill,
                                                     Time.unscaledDeltaTime * speed);
    }
}
