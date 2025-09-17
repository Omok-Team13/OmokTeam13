using System.Collections;
using TMPro;
using UnityEngine;

public class WinnerPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winnerText;

    Canvas canvas;

    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
    }

    public void WinnerNotice(string nickname)
    {
        winnerText.text = $"이번 라운드의 승자는 {nickname} 입니다";
    }

    public void finalWinnerNotice(string nickname)
    {
        winnerText.text = $"최종 승자는 {nickname} 입니다.";
    }

    public IEnumerator Hide()
    {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
}
