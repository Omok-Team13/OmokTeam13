using Controller;
using UnityEngine;
using UnityEngine.SceneManagement;

// 씬이 로드될 때 지정한 CharacterMover(또는 자동탐색된 것)을 켜고 끔.
// 에디터에서 직접 드래그해서 characterMover를 할당하는 방식이 가장 안전.
// 할당하지 않으면 "targetTag"로 자동 탐색 시도.
public class IntroToggleManager : MonoBehaviour
{
    [Header("씬 이름 설정")]
    [Tooltip("인트로 씬 이름 (이 씬에서는 CharacterMover를 비활성화)")]
    public string introSceneName = "MergeIntro";

    [Tooltip("게임(싱글룸) 씬 이름 (이 씬에서는 CharacterMover를 활성화)")]
    public string gameplaySceneName = "Single Room";

    [Header("CharacterMover 지정(Prefer)")]
    [Tooltip("직접 드래그해서 IntroCharacter의 CharacterMover 컴포넌트를 할당하세요. (권장)")]
    public MonoBehaviour CharacterMover; // CharacterMover를 드래그해서 연결

    [Header("자동탐색 옵션")]
    [Tooltip("characterMover를 할당하지 않았을 때 사용할 게임오브젝트 태그")]
    public string targetTag = "Player"; // 자동탐색용 태그

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // 플레이 모드 시작 시 현재 씬 상태에 맞춰 초기 적용
        ApplyToggleForScene(SceneManager.GetActiveScene().name);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToggleForScene(scene.name);
    }

    void ApplyToggleForScene(string sceneName)
    {
        bool isIntro = sceneName == introSceneName;
        // 우선 할당된 characterMover가 있으면 그걸 사용
        if (CharacterMover != null)
        {
            SetComponentEnabled(CharacterMover, !isIntro); // 인트로면 꺼짐 -> enabled = false
            Debug.Log($"IntroToggleManager: '{CharacterMover.name}' enabled set to {!isIntro} for scene '{sceneName}'");
            return;
        }

        // 할당이 없으면 태그로 자동 탐색 시도 (IntroCharacter 태그를 가진 오브젝트에서 CharacterMover 컴포넌트 찾기)
        GameObject target = GameObject.FindWithTag(targetTag);
        if (target != null)
        {
            var mover = target.GetComponent<CharacterMover>();
            if (mover != null)
            {
                SetComponentEnabled(mover, !isIntro);
                Debug.Log($"IntroToggleManager: Auto-found CharacterMover on '{target.name}', enabled set to {!isIntro} for scene '{sceneName}'");
                return;
            }
            else
            {
                Debug.LogWarning($"IntroToggleManager: GameObject with tag '{targetTag}' found ('{target.name}') but no CharacterMover component attached.");
            }
        }
        else
        {
            Debug.LogWarning($"IntroToggleManager: No CharacterMover assigned and no GameObject found with tag '{targetTag}'. Nothing toggled for scene '{sceneName}'.");
        }
    }

    // MonoBehaviour 타입이면 enabled 프로퍼티를 통해 켜고 끔.
    void SetComponentEnabled(MonoBehaviour comp, bool enabled)
    {
        if (comp == null) return;
        comp.enabled = enabled;
    }
}
