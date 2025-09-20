using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] Button outButton;
    [SerializeField] Button reStartButton;
    [SerializeField] GameObject winnerPanel;
    [SerializeField] Button omokRestartButton;  

    GameObject player;
    Canvas canvas;    
    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
        player = GameObject.FindWithTag("Player");

        outButton.onClick.AddListener(() =>
        {
            //나가기 버튼
            this.gameObject.SetActive(false);            
            GameManage.Instance.ChangeToIntroScene(); //인트로로 나가지기
            Destroy(player);
        });
        reStartButton.onClick.AddListener(() => //다시 하기 버튼
        {
            StateLogic.Instance.isGameEnd = false;        
            StateLogic.Instance.SetState(StateLogic.GameState.EnterOmok);
            StateLogic.Instance.RoundScore(1, true, false); //재시작
            winnerPanel.SetActive(false);            
        });

        omokRestartButton.onClick.AddListener(() =>
        {
            StateLogic.Instance.isGameEnd = false;
            StateLogic.Instance.SetState(StateLogic.GameState.Restart);
            StateLogic.Instance.RoundScore(1, true, false); //재시작
            winnerPanel.SetActive(false);
        });

        outButton.gameObject.SetActive(false);
        reStartButton.gameObject.SetActive(false);
        omokRestartButton.gameObject.SetActive(false);
    }

    public void WinnerNotice(string nickname)
    {
        winnerText.text = $"이번 라운드의 승자는 {nickname} 입니다";        
    }

    public void finalWinnerNotice(string nickname) //오목에서 이겼을 때 생성되는 팝업
    {
        winnerText.text = $"최종 승자는 {nickname} 입니다.";
        outButton.gameObject.SetActive(true);
        reStartButton.gameObject.SetActive(true);
        omokRestartButton.gameObject.SetActive(false);
    }   

    public void finalBoxingWinner(string nickname) //복싱에서 최종 승리했을 때 생성
    {
        winnerText.text = $"최종 승자는 {nickname} 입니다.";
        outButton.gameObject.SetActive(true);
        omokRestartButton.gameObject.SetActive(true);
        reStartButton.gameObject.SetActive(false);
    }


    public IEnumerator Hide()
    {
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
