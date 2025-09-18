using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD를 항상 화면에 표시하는 간단한 버전.
/// </summary>
public class TutorialUIController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Bottom HUD group (compact).")]
    public CanvasGroup hudGroup;

    private void Awake()
    {
        if (hudGroup == null)
        {
            Debug.LogWarning("TutorialUIController: assign hudGroup in inspector.");
            return;
        }

        // 항상 켜기
        hudGroup.alpha = 1f;
        hudGroup.interactable = true;
        hudGroup.blocksRaycasts = true;
    }

    // 더 이상 ShowHUD() 호출 불필요
}
