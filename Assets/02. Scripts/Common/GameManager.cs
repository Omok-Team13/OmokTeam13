using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //코드 담당자: 최은주

    //스코어 매니저 참조해서 승패 팝업 

    Constants.GameType gameT;


    public void ChangeToGameScene(Constants.GameType gameType)
    {
        gameT = gameType;
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene() //게임에서 메인 
    {
        SceneManager.LoadScene("Main");
    }

    public void ChangeToBattleScene()
    {
        // 씬 전환X 페이드 기능 사용
    }

    public void OpenSignUpPanel() //회원가입 팝업
    {

    }

    public void OpenSignInPanel() //로그인 팝업 
    {

    }     

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //씬 로드
    }
}
