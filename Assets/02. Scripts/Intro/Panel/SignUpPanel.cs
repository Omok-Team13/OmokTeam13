using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SignUpPanel : MonoBehaviour
{
    //코드 담당자 최은주
    /// <summary>
    /// 회원가입은 값이 덮어쓰기 방식이라 마지막 회원가입에 쓰인
    /// 아이디와 비밀번호를 기준으로 로그인 할 수 있습니다.
    /// </summary>

    [SerializeField] TMP_InputField userID; //플레이어가 입력하는 아이디 
    [SerializeField] TMP_InputField userPassword; //플레이어가 입력하는 비밀번호 
    [SerializeField] TMP_InputField passwordCheck; //비밀번호 확인용 
    [SerializeField] TMP_InputField nickName;

    Button confirmButton;

    private void Awake()
    {        
        //userID.text = "";              
        nickName.text = ""; //닉네임 초기화
    }

    void SaveData()
    {
        PlayerPrefs.SetString("UserID", userID.text);
        PlayerPrefs.SetString("UserName", nickName.text);
        PlayerPrefs.Save();

        if (userPassword.text == passwordCheck.text) //비밀번호가 맞을 때만 
        {
            PlayerPrefs.SetString("UserPassword", userPassword.text);
            PlayerPrefs.Save();
        }
    }

    public void Confirm() //확인 버튼에 등록할 함수
    {
        if (userPassword.text == passwordCheck.text && userPassword.text != "") //비빌번호가 맞고 빈칸이 아닐 때
        {
            if(nickName.text == "") //닉네임이 빈 상태라면
            {
                //Instantiate()
                Debug.Log("닉네임을 입력해주세요.");
                GameManager.Instance.OpenNoticePanel("닉네임을 입력해주세요.");
            }
            else if(nickName.text != "") //닉네임이 빈 상태가 아닐 때 저장
            {
                SaveData();
                PlayerPrefs.Save();

                this.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log("비밀번호가 틀렸습니다.");
            GameManager.Instance.OpenNoticePanel("비밀번호가 틀렸습니다.");
        }       
    }

    public void Inti() //초기화 
    {
        PlayerPrefs.DeleteKey("UserID");
        PlayerPrefs.DeleteKey("UserName");
        PlayerPrefs.DeleteKey("UserPassword");
        PlayerPrefs.DeleteKey("UserName");
    }
}
