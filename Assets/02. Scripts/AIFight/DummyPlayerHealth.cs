using UnityEngine;

public class DummyPlayerHealth : MonoBehaviour
{
    [Tooltip("플레이어의 현재 체력")]
    public float currentHealth = 100f;

    // 피해를 받는 함수
    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log("플레이어가 피해를 입었습니다! 현재 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 사망 처리 함수
    private void Die()
    {
        Debug.Log("플레이어가 사망했습니다.");
        // 여기에 게임 오버 로직이나 캐릭터 비활성화 코드를 추가할 수 있습니다.
        // gameObject.SetActive(false);
    }
}