using System.Collections;
using UnityEngine;

// 이 스크립트는 플레이어의 메인 GameObject에 붙입니다.
public class CyclicDummyAttack : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("주기적으로 껐다 켤 히트박스 오브젝트")]
    public GameObject playerHitboxObject;

    [Header("Attack Timing")]
    [Tooltip("히트박스가 켜져 있는 시간 (공격 판정 시간)")]
    public float attackDuration = 0.2f;

    [Tooltip("다음 공격까지의 대기 시간")]
    public float attackInterval = 1.0f;

    [Header("Attack Heights (Local Position)")]
    [Tooltip("다리 공격 시의 히트박스 높이 (Y값)")]
    public float legHeight = 0.5f;
    [Tooltip("몸통 공격 시의 히트박스 높이 (Y값)")]
    public float bodyHeight = 1.2f;
    [Tooltip("머리 공격 시의 히트박스 높이 (Y값)")]
    public float headHeight = 1.8f;

    // 현재 어떤 높이를 공격할 차례인지 추적하는 변수
    private int attackSequence = 0;

    void Start()
    {
        if (playerHitboxObject == null)
        {
            Debug.LogError("Player Hitbox Object가 연결되지 않았습니다! 인스펙터 창을 확인해주세요.");
            return;
        }

        playerHitboxObject.SetActive(false);
        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        while (true)
        {
            // 1. 공격 높이 설정
            Vector3 newPosition = playerHitboxObject.transform.localPosition;
            switch (attackSequence)
            {
                case 0: // 다리 공격
                    newPosition.y = legHeight;
                    Debug.Log("다음 공격: 다리");
                    break;
                case 1: // 몸통 공격
                    newPosition.y = bodyHeight;
                    Debug.Log("다음 공격: 몸통");
                    break;
                case 2: // 머리 공격
                    newPosition.y = headHeight;
                    Debug.Log("다음 공격: 머리");
                    break;
            }
            playerHitboxObject.transform.localPosition = newPosition;

            // 2. 히트박스 켜기
            playerHitboxObject.SetActive(true);

            // 3. 공격 판정 시간만큼 기다리기
            yield return new WaitForSeconds(attackDuration);

            // 4. 히트박스 끄기
            playerHitboxObject.SetActive(false);

            // 5. 다음 공격 순서로 넘기기
            attackSequence++;
            if (attackSequence > 2)
            {
                attackSequence = 0; // 머리 다음에는 다시 다리로
            }

            // 6. 다음 공격까지 대기
            yield return new WaitForSeconds(attackInterval);
        }
    }
}
