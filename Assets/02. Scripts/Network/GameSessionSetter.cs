using UnityEngine;

public class GameSessionSetter : MonoBehaviour
{
    [SerializeField] NetworkManager networkManager;

    public void SetGameSessionMulti()
    {
        GameSession.IsMultiplayer = true;
        Debug.Log("멀티플레이 모드 ON");

        if (networkManager)
        {
            // NetworkManager가 disabled 상태라면 일단 켠다
            networkManager.enabled = true;
            networkManager.Connect();
        }
    }

    public void SetGameSessionSingle()
    {
        GameSession.IsMultiplayer = false;
        Debug.Log("싱글플레이 모드 ON");
    }
}
