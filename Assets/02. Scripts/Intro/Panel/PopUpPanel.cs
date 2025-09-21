using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopUpPanel : MonoBehaviour
{
    [SerializeField] AudioClip omokBgm;
    [SerializeField] AudioClip introBgm;


    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] Button outButton;
    [SerializeField] Button reStartButton;
    [SerializeField] GameObject winnerPanel;
    [SerializeField] Button omokRestartButton;  

    GameObject player;
    Canvas canvas;

    bool isBoxingLose;
    private void Awake()
    {
        canvas = FindFirstObjectByType<Canvas>();
        player = GameObject.FindWithTag("Player");
        Animator playerAnim = player.GetComponent<Animator>();

        outButton.onClick.AddListener(() =>
        {
            //나가기 버튼
            this.gameObject.SetActive(false);            
            GameManage.Instance.ChangeToIntroScene(); //인트로로 나가지기
            Destroy(player);
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBGM(introBgm);
            }
            else
            {
                Debug.LogWarning("SoundManager가 존재하지 않습니다!");
            }
        });
        reStartButton.onClick.AddListener(() => //오목 다시 하기 버튼
        {
            StateLogic.Instance.isGameEnd = false;            
            GameManage.Instance.RestartScene();
            //StartCoroutine(StateLogic.Instance.RestartOmokfromOmok());
            StateLogic.Instance.RoundScore(1, true, false); //재시작
            winnerPanel.SetActive(false);
           
        });

        omokRestartButton.onClick.AddListener(() => //복싱에서 오목 다시하기
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBGM(omokBgm);
            }
            else
            {
                Debug.LogWarning("SoundManager가 존재하지 않습니다!");
            }
            if (isBoxingLose) //사망 애니메이션 중
            {
                StateLogic.Instance.isGameEnd = false;
                GameManage.Instance.RestartScene();
                StateLogic.Instance.RoundScore(1, true, false); //재시작
                playerAnim.SetTrigger("GetUp");
                winnerPanel.SetActive(false);
            }
            else //복싱 이긴 경우
            {
                StateLogic.Instance.isGameEnd = false;
                GameManage.Instance.RestartScene();
                playerAnim.SetTrigger("NoDance");
                StateLogic.Instance.RoundScore(1, true, false); //재시작
                winnerPanel.SetActive(false);
            }                  
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
        StartCoroutine(WaitButton());
    }   

    IEnumerator WaitButton()
    {
        yield return new WaitForSeconds(3f);
        outButton.gameObject.SetActive(true);
        reStartButton.gameObject.SetActive(true);
        omokRestartButton.gameObject.SetActive(false);
    }

    public void finalBoxingWinner(string nickname, bool isLose) //복싱에서 최종 승리했을 때 생성
    {
        winnerText.text = $"최종 승자는 {nickname} 입니다.";

        outButton.gameObject.SetActive(true);
        omokRestartButton.gameObject.SetActive(true);
        reStartButton.gameObject.SetActive(false);
        isBoxingLose = isLose;
    }
    public IEnumerator Hide()
    {
        yield return new WaitForSeconds(2f);
        Destroy(this.gameObject);
    }
}
