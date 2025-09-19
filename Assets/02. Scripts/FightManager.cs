using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] GameObject AIplayer;
    [SerializeField] GameObject AIomok;

    public IEnumerator AIplayerAppear()
    {
        AIomok.SetActive(false);
        yield return new WaitForSeconds(1f);

        Instantiate(AIplayer, new Vector3(1f, 0.5f, 1.5f), Quaternion.Euler(0f, 180f, 0f));

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

    public IEnumerator EndBoxing()
    {
        Destroy(AIplayer);
        yield return new WaitForSeconds(1f);
        AIomok.SetActive(true);
    }
}