using UnityEngine;
using System.Collections.Generic;

public class AttackHitbox : MonoBehaviour
{
    [Tooltip("이 공격이 가하는 피해량")]
    public float damage = 10f;

    [Tooltip("이 히트박스를 소유한 오브젝트 (공격자)")]
    public GameObject owner;

    private Collider hitboxCollider;
    private List<Collider> alreadyHitColliders = new List<Collider>();

    void Awake()
    {
        hitboxCollider = GetComponent<Collider>();
        if (hitboxCollider == null)
        {
            Debug.LogError(gameObject.name + "에서 Collider를 찾을 수 없습니다!");
        }

        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }

    public void Activate()
    {
        alreadyHitColliders.Clear();
        if (hitboxCollider != null)
            hitboxCollider.enabled = true;
    }

    public void Deactivate()
    {
        if (hitboxCollider != null)
            hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner) return;
        if (alreadyHitColliders.Contains(other)) return;

        // 공격자에 따라 피격 대상 판별
        if (owner.CompareTag("Boss") && other.CompareTag("Player"))
        {
            ApplyDamage(other, "Player");
        }
        else if (owner.CompareTag("Player") && (other.CompareTag("Boss") || other.CompareTag("Player")))
        {
            ApplyDamage(other, other.CompareTag("Boss") ? "Boss" : "Player");
        }
    }

    private void ApplyDamage(Collider other, string targetType)
    {
        alreadyHitColliders.Add(other);

        Debug.Log($"{owner.name} 의 공격이 {other.name} 에 적중! 피해량: {damage}");

        if (targetType == "Player")
        {
            // 플레이어 체력
            var playerHealth = other.GetComponent<DummyPlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(damage);

            var uiHp = other.GetComponent<DummyPlayerHealth_UI>();
            if (uiHp != null)
                uiHp.TakeDamage(damage);
        }
        else if (targetType == "Boss")
        {
            // 보스 체력
            var bossAI = other.GetComponent<BossAI>();
            if (bossAI != null)
                bossAI.TakeDamage(damage, "Body"); // 맞은 위치는 상황에 따라 Head/Body/Leg로 전달 가능
        }
    }
}
