// PlayerController.cs
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float rotateSpeed = 10f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.2f;

    [Header("References")]
    public Camera playerCamera; // 카메라가 있으면 카메라 방향 기준 이동
    public Animator animator;

    // Animator parameter names
    [Header("Animator Param Names")]
    public string paramHorizontal = "Horizontal";
    public string paramVertical = "Vertical";
    public string paramIsJump = "IsJump";
    public string paramState = "State";

    CharacterController cc;
    Vector3 velocity;
    bool isGrounded;
    float verticalVelocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        isGrounded = cc.isGrounded;
        if (isGrounded && verticalVelocity < 0f)
            verticalVelocity = -1f; // 작은 음수로 고정

        // Input
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        bool runKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jump = Input.GetButtonDown("Jump");

        Vector3 inputDir = new Vector3(h, 0f, v);
        float inputMag = Mathf.Clamp01(inputDir.magnitude);

        // 카메라 기반 이동
        Vector3 moveDir;
        if (playerCamera)
        {
            Vector3 camF = Vector3.Scale(playerCamera.transform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camR = playerCamera.transform.right;
            moveDir = (camF * v + camR * h).normalized;
        }
        else
        {
            moveDir = inputDir.normalized;
        }

        // 속도
        float targetSpeed = runSpeed * (runKey ? 1f : 0f);
        if (!runKey) targetSpeed = walkSpeed;
        Vector3 horizontalVel = moveDir * targetSpeed * inputMag;

        // 회전(방향이 있을 때만)
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
        }

        // 점프
        if (isGrounded && jump)
        {
            verticalVelocity = Mathf.Sqrt(2f * Mathf.Abs(gravity) * jumpHeight);
            if (animator != null) animator.SetBool(paramIsJump, true);
        }

        // 중력
        verticalVelocity += gravity * Time.deltaTime;
        velocity = horizontalVel + Vector3.up * verticalVelocity;

        // Move
        cc.Move(velocity * Time.deltaTime);

        // Animator 업데이트
        if (animator != null)
        {
            animator.SetFloat(paramHorizontal, h);
            animator.SetFloat(paramVertical, v);
            if (isGrounded && verticalVelocity <= 0.1f)
                animator.SetBool(paramIsJump, false);

            // 예시로 상태 설정: 0-idle,1-walk,2-run
            int state = 0;
            if (inputMag > 0.01f) state = runKey ? 2 : 1;
            animator.SetInteger(paramState, state);
        }
    }
}
