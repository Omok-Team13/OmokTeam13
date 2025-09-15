using UnityEngine;
using UnityEngine.UI;

public class Throw : MonoBehaviour
{
    [SerializeField] Transform guideTarget; // 게시판
    [SerializeField] GameObject omokBoard; // 오목판
    private Animator omokAnim;

    [SerializeField] ClothesManager clothes;
    [SerializeField] Button fightButton;

    private string faceId = "Male_emotion_angry_003";

    void Awake()
    {
        if (!clothes) clothes = GetComponentInParent<ClothesManager>();
        if (omokBoard) omokAnim = omokBoard.GetComponent<Animator>();
    }

    public void ThrowBoard()
    {
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
    }
}
