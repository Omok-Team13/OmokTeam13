using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CinemachineOnOff : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain; // Main Camera의 Brain
    [SerializeField] private PlayableDirector director; // 컷씬 타임라인

    private bool brainOffOnAwake = true; // 평소엔 수동(1인칭) 유지
    private bool reattachCameraToHeadOnEnd = true; // 컷씬 끝나면 카메라 재장착

    void Awake()
    {
        if (!brain) brain = GetComponent<CinemachineBrain>();
        if (brain && brainOffOnAwake) brain.enabled = false;
    }

    void OnEnable()
    {
        if (director) director.stopped += OnCutsceneEnd;
    }

    void OnDisable()
    {
        if (director) director.stopped -= OnCutsceneEnd;
    }

    public void PlayCutscene()
    {
        if (brain) brain.enabled = true;

        if (director)
        {
            director.time = 0;
            director.Play();
        }
    }

    private void OnCutsceneEnd(PlayableDirector _)
    {
        if (brain) brain.enabled = false;

        if (!reattachCameraToHeadOnEnd) return;

        var local = PlayerLocator.GetLocalPlayer();
        var cam = Camera.main;
        if (local && cam)
        {
            var mount = local.Head ? local.Head : local.transform;
            cam.transform.SetParent(mount, false);
            cam.transform.localPosition = Vector3.zero;
            cam.transform.localRotation = Quaternion.identity;
        }
    }

    public void ReturnToMainNow()
    {
        OnCutsceneEnd(director); // 기존 종료 로직 재사용
    }
}
