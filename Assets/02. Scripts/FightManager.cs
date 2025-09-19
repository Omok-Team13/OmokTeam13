using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] GameObject AIplayer;
    [SerializeField] GameObject AIomok;

    public IEnumerator AIplayerAppear()
    {
        yield return new WaitForSeconds(1f);
        AIomok.SetActive(false);

        Instantiate(AIplayer, new Vector3(-2f, 0.5f, -15), Quaternion.identity);

        BossAI bossAI = FindFirstObjectByType<BossAI>();


        if (bossAI != null)
        {
            bossAI.enabled = false;

            yield return new WaitForSeconds(5f);

            Animator aiAnimator = bossAI.GetComponent<Animator>();
            if (aiAnimator != null)
            {
                aiAnimator.SetTrigger("StartFight");
            }

            bossAI.enabled = true;
        }
        else
        {
            Debug.LogError("씬에서 생성된 BossAI를 찾을 수 없습니다!");
        }
    }
}