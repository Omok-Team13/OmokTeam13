using UnityEngine;
using UnityEngine.Playables;
using Unity.Cinemachine;

public class CinemachineOnOff : MonoBehaviour
{
    [SerializeField] private CinemachineBrain brain; // Main CameraÀÇ Brain
    [SerializeField] private PlayableDirector director; // ÄÆ¾À Å¸ÀÓ¶óÀÎ

    private bool brainOffOnAwake = true; // Æò¼Ò¿£ ¼öµ¿(1ÀÎÄª) À¯Áö
    private bool reattachCameraToHeadOnEnd = true; // ÄÆ¾À ³¡³ª¸é Ä«¸Þ¶ó ÀçÀåÂø

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
}
