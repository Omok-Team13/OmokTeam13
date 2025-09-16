using UnityEngine;
using UnityEngine.UI;

public class Throw : MonoBehaviour
{
    [SerializeField] Transform guideTarget; // 게시판
    [SerializeField] GameObject omokBoard; // 오목판
    private Animator omokAnim;

    [SerializeField] GameObject[] stone; // 바둑알
    #region 바둑알 전용 변수
    int stoneCount = 5; // 나타나는 바둑알 수
    float forceMin = 6f; // 튀는 최소 높이
    float forceMax = 9f; // 튀는 최대 높이
    float upwardBias = 0.5f; // 위로 살짝 치우치게
    float spreadAngle = 160f; // 퍼짐 각
    float lifeTime = 2.5f; // 자동 삭제 시간
    float spawnDelay = 0.25f; // 바둑판 흔들릴 때 바둑알 튀도록
    #endregion

    [SerializeField] ClothesManager clothes;
    [SerializeField] Button fightButton;

    private string faceId = "Male_emotion_angry_003";

    void Awake()
    {
        if (!clothes) clothes = FindFirstObjectByType<ClothesManager>();
        if (omokBoard) omokAnim = omokBoard.GetComponent<Animator>();
    }

    public void ThrowBoard()
    {
        // 오목판 제자리 (테스트용!)
        omokBoard.transform.position = new Vector3(1.03299999f, 1.62399995f, 1.89600003f);

        if (clothes == null || clothes.targetAnimator == null) return;
        if (guideTarget == null) return;

        if (clothes.catalog == null) clothes.catalog = FindFirstObjectByType<ClothesCatalog>();

        // 표정 바꾸기
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
        Invoke(nameof(SpawnStones), spawnDelay);
    }

    public void SpawnStones() // 바둑알 튀는 연출
    {
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
}
