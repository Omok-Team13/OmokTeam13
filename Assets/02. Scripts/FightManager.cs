using System.Collections;
using UnityEngine;

public class FightManager : MonoBehaviour
{

    [SerializeField] GameObject AIplayer;
    public IEnumerator AIplayerAppear() //AI 플레이어 생성
    {
        yield return new WaitForSeconds(3f);
        Instantiate(AIplayer, new Vector3(-2, 0, -15), Quaternion.identity);
    }
}
