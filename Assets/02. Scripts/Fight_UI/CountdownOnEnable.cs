using System.Collections;
using UnityEngine;
using TMPro;

public class CountdownOnEnable : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    private int start = 3; // 3부터 시작
    private float step = 0.8f; // 각 숫자 노출 시간
    private bool showFight = true; // "FIGHT!" 표시할지
    private bool autoDeactivate = false; // 끝나면 자동으로 비활성화

    void OnEnable()
    {
        StartCoroutine(Play());
    }

    IEnumerator Play()
    {
        if (!countdownText) yield break;

        for (int n = start; n >= 1; n--)
        {
            countdownText.text = n.ToString();
            yield return new WaitForSecondsRealtime(step);
        }

        if (showFight)
        {
            countdownText.text = "FIGHT!";
            yield return new WaitForSecondsRealtime(step);
        }

        if (autoDeactivate) gameObject.SetActive(false); // 타임라인에도 적용해놨는데 혹시 몰라서 같이 적용
    }
}
