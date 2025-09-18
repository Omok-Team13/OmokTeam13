using UnityEngine;
using UnityEngine.UI;

public class LogInManager : MonoBehaviour
{  
    //코드 담당자 최은주
    [SerializeField] GameObject playUI;

    Button playButton; 
    NoticePanel notice;
    Canvas canvas;

    private void Awake()
    {
        
    }

    public void CheckLogIn()
    {
        if (GameManager.Instance.loginCount == 1) //로그인 값이 모두 저장된 상태
        {
            //플레이 UI 띄우기 
            playUI.SetActive(true);
        }
        else
        {
            Debug.Log("로그인을 해야 합니다.");
            GameManager.Instance.OpenNoticePanel("로그인을 해야 합니다.");
        }
    } 
}
