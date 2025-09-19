using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] GameObject AIplayer;
    [SerializeField] GameObject AIomok;


    public IEnumerator AIplayerAppear() // AI 플레이어 생성
    {
        yield return new WaitForSeconds(1f);
        if (AIomok != null) AIomok.SetActive(false);

        // Instantiate를 새로운 변수에 저장.
        GameObject instantiatedAI = Instantiate(AIplayer, new Vector3(-2f, 0.5f, -15), Quaternion.identity);

        // 몸푸는 동안 BossAI 스크립트 비활성화
        BossAI bossAI = instantiatedAI.GetComponent<BossAI>();
        if (bossAI != null)
        {
            bossAI.enabled = false;
        }

        // 몸 푸는 시간 (5초) + 추가 대기 시간
        yield return new WaitForSeconds(0f);

        // 생성된 AI의 Animator에게 싸움 시작 신호 전달
        Animator aiAnimator = instantiatedAI.GetComponent<Animator>();
        if (aiAnimator != null)
        {
            aiAnimator.SetTrigger("StartFight");
        }

        // BossAI 스크립트를 활성화.
        if (bossAI != null)
        {
            bossAI.enabled = true;
        }
    }
}
