// ThirdPersonCamera.cs
using UnityEngine;

[AddComponentMenu("Camera/Third Person Camera")]
public class ThirdPersonCamera : MonoBehaviour
{
    [Tooltip("따라갈 대상(플레이어 Transform)")]
    public Transform target;

    [Tooltip("카메라와 대상 사이 거리")]
    public float distance = 4.0f;
    [Tooltip("카메라 높이 오프셋")]
    public float height = 1.6f;

    [Tooltip("마우스/스틱 감도")]
    public float yawSpeed = 120f;
    public float pitchSpeed = 90f;

    [Tooltip("최소/최대 피치(각도)")]
    public float minPitch = -20f;
    public float maxPitch = 60f;

    [Tooltip("회전 보간(0=즉시, 1=완전 부드러움)")]
    [Range(0f, 1f)]
    public float rotationSmoothing = 0.08f;

    private float yaw;
    private float pitch;
    private Quaternion currentRotation;

    private void Start()
    {
        if (target == null) Debug.LogWarning("ThirdPersonCamera: target is null");
        Vector3 angles = transform.eulerAngles;
        yaw = angles.y;
        pitch = angles.x;
        currentRotation = transform.rotation;
        Cursor.lockState = CursorLockMode.None;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 입력: 마우스 또는 게임패드(우측스틱)
        float inputX = Input.GetAxis("Mouse X"); // or "RightStickHorizontal" if mapped
        float inputY = Input.GetAxis("Mouse Y");

        // 모바일/게임패드 대응 필요하면 이 부분을 확장
        yaw += inputX * yawSpeed * Time.deltaTime;
        pitch -= inputY * pitchSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 목표 회전(플레이어 주위를 도는 회전)
        Quaternion targetRot = Quaternion.Euler(pitch, yaw, 0f);

        // 부드럽게 보간
        currentRotation = Quaternion.Slerp(currentRotation, targetRot, 1f - Mathf.Pow(1f - rotationSmoothing, Time.deltaTime * 60f));

        // 위치: 타겟 위치에서 뒤로 빼고 위로 올림
        Vector3 offset = currentRotation * new Vector3(0f, 0f, -distance);
        Vector3 desiredPos = target.position + Vector3.up * height + offset;
        transform.position = desiredPos;
        transform.rotation = currentRotation;
    }
}
