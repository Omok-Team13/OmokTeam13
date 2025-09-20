using Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroToggleManager : MonoBehaviour
{
    [Header("씬 이름 (대소문자/공백 무시 비교)")]
    [Tooltip("인트로 씬 이름 (이 씬에서는 대상 컴포넌트들을 비활성화)")]
    public string introSceneName = "MergeIntro";

    [Tooltip("게임플레이 씬 이름들 (이 씬들로 전환되면 CharacterMover만 활성화)")]
    public string[] gameplaySceneNames = new string[] { "SingleRoom", "Single Room" };

    [Header("직접 지정 (우선 사용)")]
    [Tooltip("직접 드래그해서 토글할 컴포넌트들을 넣으세요. (권장)")]
    public MonoBehaviour[] assignedComponents;

    [Header("자동탐색 옵션")]
    [Tooltip("할당하지 않았을 때 이 매니저가 자동으로 찾을 타입들 (비활성 오브젝트 포함)")]
    public bool autoFindDefaultTypes = true;

    [Header("디버그")]
    public bool verboseLogging = true;

    // 자동 탐색 대상 타입들
    readonly Type[] defaultTypesToFind = new Type[]
    {
        typeof(CharacterMover),
        typeof(PlayerBattleController),
        typeof(SitEmoteController)
    };

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start() => ApplyToggleForScene(SceneManager.GetActiveScene().name);

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplyToggleForScene(scene.name);

    void ApplyToggleForScene(string rawSceneName)
    {
        string sceneName = Normalize(rawSceneName);
        bool isIntro = sceneName == Normalize(introSceneName);
        bool isGameplay = gameplaySceneNames.Select(Normalize).Contains(sceneName);

        if (isIntro)
        {
            // Intro 씬: 모든 대상 컴포넌트 비활성화
            SetTargetsEnabled(false, onlyCharacterMoverEnabledInGameplay: false);
            Log($"Scene '{rawSceneName}' recognized as Intro. All target components disabled.");
            return;
        }

        if (isGameplay)
        {
            // Gameplay 씬: 오직 CharacterMover만 활성화, 나머지는 비활성화
            SetTargetsEnabled(true, onlyCharacterMoverEnabledInGameplay: true);
            Log($"Scene '{rawSceneName}' recognized as Gameplay. CharacterMover enabled; others disabled.");
            return;
        }

        LogWarning($"Scene '{rawSceneName}' not listed as Intro or Gameplay. No forced toggle performed.");
    }

    /// <summary>
    /// enabled param: 의미는 'CharacterMover를 활성화하려는 의도'일 뿐. 실제 동작은 onlyCharacterMoverEnabledInGameplay 플래그에 의해 결정됨.
    /// if onlyCharacterMoverEnabledInGameplay == true:
    ///     CharacterMover.enabled = enabled
    ///     PlayerBattleController.enabled = false
    ///     SitEmoteController.enabled = false
    /// else:
    ///     모든 대상 컴포넌트를 enabled에 따라 동일하게 설정
    /// </summary>
    void SetTargetsEnabled(bool enabled, bool onlyCharacterMoverEnabledInGameplay)
    {
        var touched = new List<string>();

        // 1) assignedComponents가 있으면 우선 처리 (각 컴포넌트의 타입에 따라 다르게 동작)
        if (assignedComponents != null && assignedComponents.Length > 0)
        {
            foreach (var comp in assignedComponents)
            {
                if (comp == null) continue;
                ApplyEnableByType(comp, enabled, onlyCharacterMoverEnabledInGameplay);
                touched.Add($"{comp.GetType().Name} on '{comp.gameObject.name}' (assigned)");
            }
            Log($"Assigned components processed: {assignedComponents.Length} items.");
        }

        // 2) 자동탐색: 씬 내의 모든 인스턴스(비활성 포함)를 찾아 강제 설정
        if (autoFindDefaultTypes)
        {
            foreach (var t in defaultTypesToFind)
            {
                try
                {
                    var found = Resources.FindObjectsOfTypeAll(t);
                    int count = 0;
                    foreach (var obj in found)
                    {
                        var mb = obj as MonoBehaviour;
                        if (mb == null) continue;

                        // 씬에 있는 인스턴스만 처리 (에셋/프리팹 제외)
                        if (!mb.gameObject.scene.IsValid()) continue;

                        ApplyEnableByType(mb, enabled, onlyCharacterMoverEnabledInGameplay);
                        count++;
                        touched.Add($"{t.Name} on '{mb.gameObject.name}' (auto)");
                    }
                    Log($"Auto-find: processed {count} '{t.Name}' instance(s).");
                }
                catch (Exception ex)
                {
                    LogWarning($"Error while auto-finding type '{t.Name}': {ex.Message}");
                }
            }
        }

        if (verboseLogging && touched.Count > 0)
        {
            Debug.Log($"[IntroToggleManagerV4] Toggled components:\n - {string.Join("\n - ", touched)}");
        }
    }

    void ApplyEnableByType(MonoBehaviour mb, bool enabled, bool onlyCharacterMoverEnabledInGameplay)
    {
        if (mb == null) return;

        var typeName = mb.GetType().Name;

        if (onlyCharacterMoverEnabledInGameplay)
        {
            // Gameplay 의도: CharacterMover만 enabled로, 나머지는 false
            if (typeName == nameof(CharacterMover))
            {
                mb.enabled = enabled;
            }
            else
            {
                mb.enabled = false;
            }
        }
        else
        {
            // Intro 또는 일반적 강제 세팅: 모든 대상 컴포넌트를 enabled 값으로 설정
            mb.enabled = enabled;
        }
    }

    string Normalize(string s) => (s ?? "").Trim().ToLowerInvariant();

    void Log(string msg) { if (verboseLogging) Debug.Log($"[IntroToggleManagerV4] {msg}"); }
    void LogWarning(string msg) { Debug.LogWarning($"[IntroToggleManagerV4] {msg}"); }
}
