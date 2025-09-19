using System.Collections.Generic;
using UnityEngine;

public class DummyPlayerHealth : MonoBehaviour
{
    [Tooltip("플레이어의 현재 체력")]
    public float currentHealth = 100f;
    private bool isDead = false;
    private Animator animator;

    [Header("Hitbox References")]
    [Tooltip("오른손 히트박스의 AttackHitbox 스크립트")]
    public AttackHitbox rightHandHitbox;
    [Tooltip("왼손 히트박스의 AttackHitbox 스크립트")]
    public AttackHitbox leftHandHitbox;
    [Tooltip("오른발 히트박스의 AttackHitbox 스크립트")]
    public AttackHitbox rightFootHitbox;
    [Tooltip("왼발 히트박스의 AttackHitbox 스크립트")]
    public AttackHitbox leftFootHitbox;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnHitbox(string hitboxName)
    {
        switch (hitboxName)
        {
            case "RightHand": if (rightHandHitbox != null) rightHandHitbox.Activate(); break;
            case "LeftHand": if (leftHandHitbox != null) leftHandHitbox.Activate(); break;
            case "RightFoot": if (rightFootHitbox != null) rightFootHitbox.Activate(); break;
            case "LeftFoot": if (leftFootHitbox != null) leftFootHitbox.Activate(); break;
        }
    }

    public void OffHitbox(string hitboxName)
    {
        switch (hitboxName)
        {
            case "RightHand": if (rightHandHitbox != null) rightHandHitbox.Deactivate(); break;
            case "LeftHand": if (leftHandHitbox != null) leftHandHitbox.Deactivate(); break;
            case "RightFoot": if (rightFootHitbox != null) rightFootHitbox.Deactivate(); break;
            case "LeftFoot": if (leftFootHitbox != null) leftFootHitbox.Deactivate(); break;
            case "All":
                if (rightHandHitbox != null) rightHandHitbox.Deactivate();
                if (leftHandHitbox != null) leftHandHitbox.Deactivate();
                if (rightFootHitbox != null) rightFootHitbox.Deactivate();
                if (leftFootHitbox != null) leftFootHitbox.Deactivate();
                break;
        }
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
        StateLogic.Instance.CheckHP(true, false); //죽었다고 알려주기
    }
}