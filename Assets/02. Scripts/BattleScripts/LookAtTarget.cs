using UnityEngine;

public class LookAtTarget : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f; // 회전 속도

    private Transform target;

    void Start()
    {
        // 우선순위: Player > Boss
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            return;
        }

        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            target = boss.transform;
        }
    }

    void Update()
    {
        if (target == null) return;

        // 방향 계산
        Vector3 direction = target.position - transform.position;
        direction.y = 0f; // y축 회전만 (위/아래 고개 끄덕임 제외)

        if (direction.magnitude > 0.1f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
