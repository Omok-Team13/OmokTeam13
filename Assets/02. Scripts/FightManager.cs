using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{
    [SerializeField] GameObject AIplayer;
    [SerializeField] GameObject AIomok;
 
 
    public IEnumerator AIplayerAppear() //AI 플레이어 생성
    {
        yield return new WaitForSeconds(1f);
        AIomok.SetActive(false);
        Instantiate(AIplayer, new Vector3(-2f, 0.5f, -15), Quaternion.identity);
        AIplayer.GetComponent<Animator>().enabled = false;
        yield return new WaitForSeconds(5f);
        AIplayer.GetComponent<Animator>().enabled = true;        
    }
}
