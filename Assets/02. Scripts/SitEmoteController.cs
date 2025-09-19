using UnityEngine;

public class SitEmoteController : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // 숫자키 1~4 입력 시 애니메이션 실행
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetTrigger("Emote1");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.SetTrigger("Emote2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            animator.SetTrigger("Emote3");
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            animator.SetTrigger("Emote4");
        }
    }
}
