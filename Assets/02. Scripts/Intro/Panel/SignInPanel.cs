using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignInPanel : MonoBehaviour
{
    [SerializeField] TMP_InputField playerID; //플레이어가 입력하는 아이디 
    [SerializeField] TMP_InputField playerPassword; //플레이어가 입력하는 비밀번호
    [SerializeField] Button ConfirmButton;

    string userID; //저장된 아이디 
    string userPassword; //저장된 비밀번호  
    string userName; //저장된 닉네임 

    private void Awake()
    {
        ConfirmButton.onClick.AddListener(Login); //버튼에 함수 연결
    }    

    private void Login()
    {
        userID = PlayerPrefs.GetString("UserID");
        userPassword = PlayerPrefs.GetString("UserPassword");
        userName = PlayerPrefs.GetString("UserName");
        //아이디 비밀번호 확인용으로 잠시 넣어뒀습니다
        //if (PlayerPrefs.HasKey("UserID") || PlayerPrefs.HasKey("UserPassword")) 
        //{
        //    Debug.Log(userID);
        //    Debug.Log(userPassword);
        //}

        if (playerID.text != userID) //플레이어가 입력한 아이디가 저장값과 맞지 않을 때
        {
            Debug.Log("아이디가 존재하지 않습니다.");
            GameManage.Instance.OpenNoticePanel("아이디가 존재하지 않습니다.");
        }
        else if (playerPassword.text != userPassword) //플레이어가 입력한 비밀번호가 맞지 않을 때
        {
            Debug.Log("비밀번호가 일치하지 않습니다.");
            GameManage.Instance.OpenNoticePanel("비밀번호가 일치하지 않습니다.");
        }
        else if (playerID.text == userID && playerPassword.text == userPassword)
        {
            Debug.Log("로그인에 성공했습니다.");
            GameManage.Instance.OpenNoticePanel("로그인에 성공했습니다.");
            GameObject.FindWithTag("Player").gameObject.GetComponent<PlayerState>().nameSet(userName);

            this.gameObject.SetActive(false);
            GameManage.Instance.loginCount = 1;
        }
    }
}
