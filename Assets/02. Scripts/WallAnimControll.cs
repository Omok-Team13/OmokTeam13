using System.Collections;
using UnityEngine;

public class WallAnimControll : MonoBehaviour
{
    //코드 담당자: 최은주

    [SerializeField] GameObject[] walls; //넘어질 벽 배열 
    [SerializeField] GameObject[] basicWalls; //기존 가림막 벽 배열
    [SerializeField] Transform[] smokePos; //연기 나오는 이펙트 위치
    [SerializeField] GameObject gameRoom; //전체 방 
    [SerializeField] GameObject top; //천장 
    [SerializeField] GameObject boxingArena; //복싱장
    [SerializeField] GameObject guide; //안내문
    [SerializeField] GameObject Wallobject;  //벽 오브젝트
    [SerializeField] GameObject smokePrefab;

    /// <summary>
    ///  안내문은 이벤트 쪽에 들어있는 게 좋을 것 같아서 벽 오브젝트 쪽에다가  
    /// 안 옮기고 그냥 따로 할당해주었습니다. 추후 배틀 모드 진입할 때 여기 있는 함수 참조해서 등록하면 될 것 같아요.
    /// </summary>
   
    public void WallFallOver() //벽 무너지는 애니메이션 
    {
        for (int i = 0; i < walls.Length; i++)
        {
            for (int j = 0; j < basicWalls.Length; j++) //기본 가벽 사라지기
            {
                basicWalls[j].SetActive(false);
            }

            if (walls[i] == null)
            {
                Debug.LogError($"walls[{i}] is null!"); 
                continue;
            }
            Animator anim = walls[i].GetComponent<Animator>();
            if (anim == null)
            {
                Debug.LogError($"walls[{i}] 애니메이션 없음");
                continue;
            }             
            anim.SetTrigger("Fall");        
            top.SetActive(false);
        }
        Wallobject.SetActive(false);
        guide.SetActive(false);
        StartCoroutine(SmokeEffect());
        StartCoroutine(IntoBoxingArena());
    }

    IEnumerator IntoBoxingArena() //잠시 기다리기 위해 코루틴
    {
        yield return new WaitForSeconds(2.5f);
        boxingArena.SetActive(true);       
        gameRoom.SetActive(false);
    }

    IEnumerator SmokeEffect()
    {
        yield return new WaitForSeconds(0.3f);
        for(int i = 0; i<1; i++)
        {
            for(int j = 0; j<smokePos.Length; j++)
            {
                Instantiate(smokePrefab, smokePos[j]);
            }
        }
    }
}
