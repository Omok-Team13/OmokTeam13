using UnityEngine;

/// <summary>
/// FirstPersonController (mouse optional look)
/// - CharacterController 필요
/// - 카메라(Head)는 플레이어의 자식으로 두고 inspector의 playerCamera에 할당
/// - LookInputMode로 마우스/키보드/게임패드/비활성 선택 가능
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public enum LookInputMode { Mouse, Keyboard, Gamepad, Disabled }

    [Header("References")]
    [SerializeField] private Camera playerCamera = null; // 씬의 카메라(Head). Inspector에 할당

    [Header("Movement (m/s)")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 6.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Look Settings")]
    [SerializeField] private LookInputMode lookMode = LookInputMode.Mouse;
    [SerializeField, Tooltip("마우스 민감도 X (수평)")] private float mouseSensitivityX = 120f;
    [SerializeField, Tooltip("마우스 민감도 Y (수직)")] private float mouseSensitivityY = 120f;
    [SerializeField, Tooltip("키보드 회전 속도 (deg/sec)")] private float keyboardYawSpeed = 90f;
    [SerializeField, Tooltip("키보드 피치 속도 (deg/sec)")] private float keyboardPitchSpeed = 60f;
    [SerializeField, Tooltip("게임패드 축 이름 (수평)")] private string gamepadAxisX = "RightStickHorizontal";
    [SerializeField, Tooltip("게임패드 축 이름 (수직)")] private string gamepadAxisY = "RightStickVertical";

    [SerializeField, Tooltip("시선 위쪽 한계(도)")] private float maxPitch = 75f;
    [SerializeField, Tooltip("시선 아래쪽 한계(도)")] private float minPitch = -75f;
    [SerializeField, Range(0f, 1f), Tooltip("마우스/스틱 스무딩(0=즉시)")] private float rotationSmoothing = 0.0f;

    [Header("Options")]
    [SerializeField] private bool lockCursorOnStart = true;
    [SerializeField] private bool canMove = true;

    // keyboard keys (customizable in inspector)
    [Header("Keyboard Look Keys (if using Keyboard mode)")]
    [SerializeField] private KeyCode yawLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode yawRightKey = KeyCode.E;
    [SerializeField] private KeyCode pitchUpKey = KeyCode.R;
    [SerializeField] private KeyCode pitchDownKey = KeyCode.F;

    // internal
    private CharacterController controller;
    private Vector3 velocity;
    private float currentYaw;
    private float currentPitch;
    private Vector2 currentMouseDeltaSmooth;
    private Vector2 mouseDeltaSmoothVel;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>();
            if (playerCamera == null)
                Debug.LogWarning("FirstPersonController: playerCamera가 할당되어 있지 않습니다. Inspector에 카메라를 할당하세요.");
        }

        Vector3 e = transform.eulerAngles;
        currentYaw = e.y;
        currentPitch = playerCamera ? playerCamera.transform.localEulerAngles.x : 0f;

        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void Update()
    {
        HandleLookInput();
        HandleMovement();
    }

    private void HandleLookInput()
    {
        float mx = 0f;
        float my = 0f;

        switch (lookMode)
        {
            case LookInputMode.Mouse:
                mx = Input.GetAxis("Mouse X");
                my = Input.GetAxis("Mouse Y");
                // apply smoothing optionally
                if (rotationSmoothing > 0f)
                {
                    Vector2 raw = new Vector2(mx, my);
                    currentMouseDeltaSmooth = Vector2.SmoothDamp(currentMouseDeltaSmooth, raw, ref mouseDeltaSmoothVel, rotationSmoothing);
                    mx = currentMouseDeltaSmooth.x;
                    my = currentMouseDeltaSmooth.y;
                }
                currentYaw += mx * mouseSensitivityX * Time.deltaTime;
                currentPitch -= my * mouseSensitivityY * Time.deltaTime;
                break;

            case LookInputMode.Keyboard:
                // yaw left/right
                float yawDir = 0f;
                if (Input.GetKey(yawLeftKey)) yawDir -= 1f;
                if (Input.GetKey(yawRightKey)) yawDir += 1f;
                // pitch up/down
                float pitchDir = 0f;
                if (Input.GetKey(pitchUpKey)) pitchDir += 1f;
                if (Input.GetKey(pitchDownKey)) pitchDir -= 1f;

                currentYaw += yawDir * keyboardYawSpeed * Time.deltaTime;
                currentPitch += pitchDir * keyboardPitchSpeed * Time.deltaTime;
                break;

            case LookInputMode.Gamepad:
                // Input axes should be configured in Input Manager (or new Input System)
                float gx = 0f;
                float gy = 0f;
                if (!string.IsNullOrEmpty(gamepadAxisX)) gx = Input.GetAxis(gamepadAxisX);
                if (!string.IsNullOrEmpty(gamepadAxisY)) gy = Input.GetAxis(gamepadAxisY);

                // optional smoothing
                if (rotationSmoothing > 0f)
                {
                    Vector2 raw = new Vector2(gx, gy);
                    currentMouseDeltaSmooth = Vector2.SmoothDamp(currentMouseDeltaSmooth, raw, ref mouseDeltaSmoothVel, rotationSmoothing);
                    gx = currentMouseDeltaSmooth.x;
                    gy = currentMouseDeltaSmooth.y;
                }

                currentYaw += gx * mouseSensitivityX * Time.deltaTime;
                currentPitch -= gy * mouseSensitivityY * Time.deltaTime;
                break;

            case LookInputMode.Disabled:
                // do nothing
                break;
        }

        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        // apply rotation: yaw on the player transform, pitch on camera local rotation
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }

    private void HandleMovement()
    {
        if (!canMove) return;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        Vector3 camForward;
        Vector3 camRight;
        if (playerCamera != null)
        {
            camForward = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(playerCamera.transform.right, Vector3.up).normalized;
        }
        else
        {
            camForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
            camRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        }

        Vector3 inputDir = camRight * h + camForward * v;
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        float targetSpeed = isRunning ? runSpeed : walkSpeed;
        Vector3 horizontalVelocity = inputDir * targetSpeed;

        if (controller.isGrounded)
        {
            if (velocity.y < 0f) velocity.y = -1f;
            if (jumpPressed)
            {
                velocity.y = Mathf.Sqrt(2f * Mathf.Abs(gravity) * Mathf.Max(0f, jumpHeight));
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        Vector3 finalVelocity = horizontalVelocity + Vector3.up * velocity.y;
        controller.Move(finalVelocity * Time.deltaTime);
    }

    #region Public API
    public void SetCanMove(bool enable) => canMove = enable;
    public void SetCursorLocked(bool locked)
    {
        if (locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void SetLookMode(LookInputMode mode) => lookMode = mode;
    #endregion
}
