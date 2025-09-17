using UnityEngine;
using System.Collections;

public class PopUp_KnockOut : MonoBehaviour
{
    [Header("팝업 패널을 여기다 드래그해서 넣어주세요")]
    public GameObject KnockOutPanel;

    private Coroutine autoCloseCoroutine;

    // 기본 팝업 열기 (자동 닫기 없음)
    public void OpenPopup()
    {
        if (KnockOutPanel != null)
        {
            KnockOutPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("KnockOutPanel이 Inspector에서 연결되지 않았습니다!");
        }
    }

    // 일정 시간 후 자동으로 닫히는 팝업 열기
    public void OpenPopup(float duration)
    {
        if (KnockOutPanel != null)
        {
            KnockOutPanel.SetActive(true);

            // 이미 실행 중인 코루틴이 있으면 정지
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);

            // 자동 닫기 코루틴 실행
            autoCloseCoroutine = StartCoroutine(AutoClose(duration));
        }
        else
        {
            Debug.LogError("KnockOutPanel이 Inspector에서 연결되지 않았습니다!");
        }
    }

    // 팝업 닫기
    public void ClosePopup()
    {
        if (KnockOutPanel != null)
        {
            KnockOutPanel.SetActive(false);

            // 닫을 때 코루틴도 정리
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
        else
        {
            Debug.LogError("KnockOutPanel이 Inspector에서 연결되지 않았습니다!");
        }
    }

    // 자동 닫기 코루틴
    private IEnumerator AutoClose(float duration)
    {
        yield return new WaitForSeconds(duration);
        ClosePopup();
    }
}
