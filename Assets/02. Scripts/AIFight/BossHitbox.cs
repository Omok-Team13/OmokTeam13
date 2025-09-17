using UnityEngine;
using System.Collections.Generic;

// 이 스크립트는 각 히트박스 오브젝트(예: RightHand_Hitbox)에 붙입니다.
public class BossHitbox : MonoBehaviour
{
    [Tooltip("이 공격이 가하는 피해량")]
    public float damage = 10f;

    // 자신의 콜라이더를 저장할 변수
    private Collider hitboxCollider;
    // 한 번의 공격으로 여러 번 피해가 들어가는 것을 방지하기 위한 리스트
    private List<Collider> alreadyHitColliders = new List<Collider>();

    void Awake()
    {
        // 시작할 때 자신의 콜라이더 컴포넌트를 찾아 저장해둡니다.
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
        {
            Debug.LogError(gameObject.name + "에서 Collider를 찾을 수 없습니다!");
        }
        // 시작 시에는 비활성화
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    // BossAI가 호출할 활성화 함수
    public void Activate()
    {
        // 이전에 맞았던 대상을 초기화하고
        alreadyHitColliders.Clear();
        // 콜라이더를 켭니다.
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
        }
    }

    // BossAI가 호출할 비활성화 함수
    public void Deactivate()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    // Is Trigger가 켜진 콜라이더에 다른 콜라이더가 들어왔을 때 호출됩니다.
    private void OnTriggerEnter(Collider other)
    {
        // 플레이어에게 닿았고, 이번 공격에서 아직 때린 적이 없다면
        if (other.CompareTag("Player") && !alreadyHitColliders.Contains(other))
        {
            // 맞은 대상을 리스트에 추가하여 중복 피격을 방지합니다.
            alreadyHitColliders.Add(other);

            // 이 스크립트에 설정된 고정 damage 값을 사용합니다.
            Debug.Log(this.name + "가 플레이어에게 명중! 피해량: " + damage);

            // DummyPlayerHealth 스크립트의 TakeDamage 함수를 호출합니다.
            DummyPlayerHealth playerHealth = other.GetComponent<DummyPlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}

