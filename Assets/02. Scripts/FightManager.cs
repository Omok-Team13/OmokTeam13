using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] GameObject AIplayer;
    [SerializeField] GameObject AIomok;
    public IEnumerator AIplayerAppear() //AI 플레이어 생성
    {
        yield return new WaitForSeconds(3f);
        Instantiate(AIplayer, new Vector3(-2f, 0.5f, -15), Quaternion.identity);
        AIomok.SetActive(false);
    }
}
