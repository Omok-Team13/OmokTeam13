using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinnerPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] Button outButton;
    [SerializeField] Button reStartButton;
    [SerializeField] GameObject winnerPanel;

    Canvas canvas;    

    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();

        outButton.onClick.AddListener(() =>
        {
            //나가기 버튼
            this.gameObject.SetActive(false);
            GameManage.Instance.ChangeToIntroScene(); //인트로로 나가지기

        });
        reStartButton.onClick.AddListener(() => //다시 하기 버튼
        {
            StateLogic.Instance.isGameEnd = false;
            var nextState = GameManage.Instance.GetState(0);
            StateLogic.Instance.SetState(nextState);
            StateLogic.Instance.RoundScore(1, true); //재시작
            winnerPanel.SetActive(false);
        });

        outButton.gameObject.SetActive(false);
        reStartButton.gameObject.SetActive(false);
    }

    public void WinnerNotice(string nickname)
    {
        winnerText.text = $"이번 라운드의 승자는 {nickname} 입니다";        
    }

    public void finalWinnerNotice(string nickname)
    {
        winnerText.text = $"최종 승자는 {nickname} 입니다.";
        outButton.gameObject.SetActive(true);
        reStartButton.gameObject.SetActive(true);
    }

    public IEnumerator Hide()
    {
        yield return new WaitForSeconds(2f);
        this.gameObject.SetActive(false);
    }
}
