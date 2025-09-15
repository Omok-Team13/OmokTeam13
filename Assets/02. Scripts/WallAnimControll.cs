using System.Collections;
using UnityEngine;

public class WallAnimControll : MonoBehaviour
{
    //코드 담당자: 최은주

    [SerializeField] GameObject[] walls; //넘어질 벽 배열 
    [SerializeField] GameObject[] basicWalls; //기존 가림막 벽 배열
    [SerializeField] GameObject gameRoom; //전체 방 
    [SerializeField] GameObject top; //천장 
    [SerializeField] GameObject boxingArena; //복싱장
    [SerializeField] GameObject guide; //안내문
    [SerializeField] GameObject Wallobject;  //벽 오브젝트
    [SerializeField] GameObject smokePrefab;
    [SerializeField] Transform smokePos;

    /// <summary>
    ///  안내문은 이벤트 쪽에 들어있는 게 좋을 것 같아서 벽 오브젝트 쪽에다가  
    /// 안 옮기고 그냥 따로 할당해주었습니다 
    /// </summary>

    public void Awake()
    {
        StartCoroutine(Wait());

    }

    public void Start() //일단은 스타트인데 나중에 상태 관리에서로 켜주면 될 듯합니다.
    {       

        for(int i= 0; i < walls.Length; i++)
        {     
            for(int j = 0; j<basicWalls.Length; j++)
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

            //Debug.Log($"Triggering Fall on wall {i}");     
            anim.SetTrigger("Fall");
            top.SetActive(false);

        }
        Wallobject.SetActive(false);
        guide.SetActive(false);

        StartCoroutine(IntoBoxingArena());
    }


    public void Change()
    {
        for (int i = 0; i < walls.Length; i++)
        {
            for (int j = 0; j < basicWalls.Length; j++)
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

            //Debug.Log($"Triggering Fall on wall {i}");     
            anim.SetTrigger("Fall");
            top.SetActive(false);

        }
        Wallobject.SetActive(false);
        guide.SetActive(false);

        StartCoroutine(IntoBoxingArena());
    }

    IEnumerator IntoBoxingArena() //4초 기다리기 위해 코루틴
    {
        boxingArena.SetActive(true);
        Instantiate(smokePrefab, smokePos);
        yield return new WaitForSeconds(2f);
        gameRoom.SetActive(false);
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(3f);
    }
}
