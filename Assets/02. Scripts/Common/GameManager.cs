using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    //�ڵ� �����: ������

    //���ھ� �Ŵ��� �����ؼ� ���� �˾� 

    Constants.GameType gameT;


    public void ChangeToGameScene(Constants.GameType gameType)
    {
        gameT = gameType;
        SceneManager.LoadScene("Game");
    }

    public void ChangeToMainScene() //���ӿ��� ���� 
    {
        SceneManager.LoadScene("Main");
    }

    public void ChangeToBattleScene()
    {
        // �� ��ȯX ���̵� ��� ���
    }

    public void OpenSignUpPanel() //ȸ������ �˾�
    {

    }

    public void OpenSignInPanel() //�α��� �˾� 
    {

    }     

    protected override void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        //�� �ε�
    }
}
