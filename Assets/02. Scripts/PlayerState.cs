using UnityEngine;

public class PlayerState : MonoBehaviour
{
    //�ڵ� �����: ������

    bool isFirstP;

    Constants.PlayerType playerType;
    //���� ��� �޾ƿ��� 

    //UI �г� �ø���������ʵ�� �޾ƿ���

    public PlayerState(bool isFirstPlayer) //������ 
    {
        isFirstP = isFirstPlayer;
        playerType = isFirstP ? Constants.PlayerType.PlayerA : Constants.PlayerType.PlayerB;
    }

    #region ���� �޼���
    public void EnterGame()
    {
        //���� �����ϰ� �����ϴ� �� ���� ǥ�� UI. 

        //��Ʋ ��û ��ư UI
    }


    public void ExitGame()
    {
        //���� ������ ��
    }

    public void HandleMove() //�� ����
    {

    }

    public void NextTurn() //�� �ѱ�� 
    {

    }

    #endregion
}
