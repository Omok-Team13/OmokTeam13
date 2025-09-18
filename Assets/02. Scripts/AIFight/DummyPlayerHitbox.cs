using UnityEngine;

// 이 스크립트는 플레이어의 각 히트박스 오브젝트(예: Player_RightHand_Hitbox)에 붙입니다.
public class PlayerHitbox : MonoBehaviour
{
    [Tooltip("이 공격이 가하는 피해량")]
    public float damage = 10f;

    // Is Trigger가 켜진 콜라이더에 다른 콜라이더가 들어왔을 때 호출됩니다.
    private void OnTriggerEnter(Collider other)
    {
        // 보스의 피격 부위(Hurtbox) 태그 중 하나와 충돌했는지 확인합니다.
        // FSM을 만들 때 보스 뼈대에 설정했던 태그들입니다.
        if (other.CompareTag("Head") || other.CompareTag("Body") || other.CompareTag("Leg"))
        {
            // 충돌한 오브젝트의 최상위 부모(보스 메인 오브젝트)에서 BossAI 스크립트를 찾습니다.
            BossAI bossAI = other.transform.root.GetComponent<BossAI>();

            if (bossAI != null)
            {
                // "BossHead" 태그에서 "Boss"를 제거하여 "Head"만 남깁니다.
                string hitLocation = other.tag;

                Debug.Log("보스의 " + hitLocation + " 부위에 명중! 피해량: " + damage);

                // BossAI 스크립트의 TakeDamage 함수를 호출하여 피해를 입힙니다.
                bossAI.TakeDamage(damage, hitLocation);

                // 한 번의 공격으로 여러 부위가 동시에 맞는 것을 방지하고,
                // 정확한 공격 판정을 위해 이 히트박스를 즉시 비활성화합니다.
                // (부모의 DummyPlayerAttack 스크립트가 다음 공격 때 다시 켜줄 것입니다.)
                gameObject.SetActive(false);
            }
        }
    }
}

