using UnityEngine;

public class DummyPlayerHealth : MonoBehaviour
{
    [Tooltip("플레이어의 현재 체력")]
    public float currentHealth = 100f;
    private bool isDead = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    // 피해를 받는 함수
    public void TakeDamage(float damageAmount)
    {
        if (isDead == true)
        {
            return;
        }
        currentHealth -= damageAmount;
        if (currentHealth < 0) { currentHealth = 0; }
        Debug.Log("플레이어가 피해를 입었습니다! 현재 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Debug.Log("플레이어가 사망했습니다. 플레이어 패배");
            Die();
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        isDead = true;
        Debug.Log("플레이어가 사망했습니다.");
        animator.SetTrigger("Die");
        this.enabled = false;
    }
}