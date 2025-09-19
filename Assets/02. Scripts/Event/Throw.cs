using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 이 스크립트는 캐릭터 오브젝트에 넣어두었습니다.
/// 그래서 Manager같은 오브젝트에 넣지 말아주세요 (중복되면 던지는 로직이 꼬일 수 있음)
/// </summary>

public class Throw : MonoBehaviour
{
    #region 던지는 애니메이션 동작을 위해 필요한 것들
    [SerializeField] Transform guideTarget; // 게시판
    [SerializeField] GameObject omokBoard; // 오목판
    private Animator omokAnim; // 오목판 애니메이션

    [SerializeField] GameObject[] stone; // 바둑알

    [SerializeField] ClothesManager clothes; // 캐릭터 표정 변화를 위해 필요한 변수
    #endregion

    Animator playerAnim;

    private void Awake()
    {
        playerAnim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;    

    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StartCoroutine(ConnectionFrame()); // 로드 직후엔 못 찾는 문제 해결용
    }

    public void ThrowBoard()
    {
        // 인스펙터 자동 연결
        ConnectionPlayerRefs();
        ConnectionThrowRefs();

        if (!omokBoard || !guideTarget || !clothes || !clothes.targetAnimator)
            return;
        if (clothes.catalog == null)
            clothes.catalog = GetComponentInChildren<ClothesCatalog>(true);

        // 오목판 제자리 (테스트용)
        //omokBoard.transform.position = new Vector3(1.03299999f, 1.62399995f, 1.89600003f);

        // 표정 바꾸기
        string faceId = "Male_emotion_angry_003";
        clothes.Unequip(SlotType.Faces);
        clothes.EquipById(faceId);

        var anim = clothes.targetAnimator;

        var playerTf = clothes && clothes.targetAnimator
              ? clothes.targetAnimator.transform
              : transform;

        Vector3 toBoard = guideTarget.position - playerTf.position; // 게시판 위치
        Vector3 actorRight = playerTf.right; // 캐릭터 오브젝트 기준으로 좌/우 판정

        bool MirrorThrow = false;
        MirrorThrow = Vector3.Dot(actorRight, toBoard) < 0f; // 왼쪽이면 true, 오른쪽이면 false

        anim.SetBool("Mirror", MirrorThrow);
        anim.SetTrigger("Throw");

        if (omokAnim) omokAnim.SetTrigger("Throw");
        Invoke(nameof(SpawnStones), 0.25f);        
        StartCoroutine(waitTillThrow());
    }

    public void SpawnStones() // 바둑알 튀는 연출
    {
        #region 바둑알 전용 변수
        int stoneCount = 5; // 나타나는 바둑알 수
        float forceMin = 6f; // 튀는 최소 높이
        float forceMax = 9f; // 튀는 최대 높이
        float upwardBias = 0.5f; // 위로 살짝 치우치게
        float spreadAngle = 160f; // 퍼짐 각
        float lifeTime = 2.5f; // 자동 삭제 시간
        #endregion

        if (omokBoard == null || stone == null || stone.Length == 0) return;

        var origin = omokBoard.transform.position;
        var forward = omokBoard.transform.forward;

        for (int i = 0; i < stoneCount; i++)
        {
            var prefab = stone[Random.Range(0, stone.Length)]; // 검은알, 흰알 중에 하나 랜덤으로 나타나게
            var go = Instantiate(prefab, origin, Random.rotation);

            // 3D 물리
            var rb = go.GetComponent<Rigidbody>();
            if (!rb) rb = go.AddComponent<Rigidbody>(); // 프리팹에 넣어두긴 했는데 혹시 몰라서 넣어둠

            // 퍼짐 방향 계산
            var randYaw = Random.Range(-spreadAngle, spreadAngle);
            var dir = Quaternion.AngleAxis(randYaw, Vector3.up) * forward;
            dir = (dir + Vector3.up * upwardBias).normalized;

            var force = Random.Range(forceMin, forceMax);
            rb.AddForce(dir * force, ForceMode.Impulse);

            Destroy(go, lifeTime);
        }
    }

    // 멀티플레이를 위해 인스펙터 자동 연결
    void ConnectionPlayerRefs()
    {
        if (clothes) return;

        if (!TryGetComponent(out clothes)) // 자기 자신 오브젝트에서 ClothesManager 찾기
            clothes = GetComponentInChildren<ClothesManager>(true);
    }

    // 씬 전환 시 문제없도록 인스펙터 자동 연결
    void ConnectionThrowRefs()
    {
        if (!guideTarget)
        {
            var t = FindByTagAllScenes("Guide");
            if (t) guideTarget = t;
        }

        if (!omokBoard)
        {
            var t = FindByTagAllScenes("OmokBoard");
            if (t) omokBoard = t.gameObject;
        }
    }

    // 새로 로드된 씬에서 오브젝트 탐색 (캐릭터가 DontDestoryOnLoad로 가져와져서)
    Transform FindByTagAllScenes(string tag)
    {
        for (int s = 0; s < SceneManager.sceneCount; s++)
        {
            var sc = SceneManager.GetSceneAt(s);
            if (!sc.isLoaded) continue;

            foreach (var root in sc.GetRootGameObjects())
            {
                var trs = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < trs.Length; i++)
                    if (trs[i].CompareTag(tag)) return trs[i];
            }
        }
        return null;
    }

    IEnumerator ConnectionFrame()
    {
        yield return null;
        ConnectionThrowRefs(); // 재탐색
        if (!omokAnim && omokBoard)
            omokAnim = omokBoard.GetComponent<Animator>();
    }

    IEnumerator waitTillThrow()
    {
        yield return new WaitForSeconds(1.5f);
        playerAnim.SetTrigger("Stand");
    }
}
