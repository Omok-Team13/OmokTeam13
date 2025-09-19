using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [SerializeField] string roomName = "Multi Room";
    [SerializeField] byte maxPlayers = 2;
    private string gameVersion = "1";

    void Awake()
    {
        Screen.SetResolution(1920, 1080, false); // 해상도 설정, false = Full Screen 사용 여부
        PhotonNetwork.SendRate = 60; // 내 컴퓨터 게임 정보에 대한 전송률
        PhotonNetwork.SerializationRate = 30; // Photon View 관측 중인 대상에 대한 전송률
        PhotonNetwork.AutomaticallySyncScene = true; // 씬 동기화
        PhotonNetwork.GameVersion = gameVersion;
    }

    void Start()
    {
        if (!GameSession.IsMultiplayer)
        {
            enabled = false;               // 이 컴포넌트 비활성
            return;
        }
        Connect();
    }

    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings(); // App ID 기반으로 접속

        Debug.Log("서버 접속");
    }

    public override void OnConnectedToMaster()
    {
        var opts = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);

        Debug.Log("서버 접속 완료");
    }

    public override void OnJoinedRoom()
    {
        if (!GameSession.IsMultiplayer) // 혹시라도 싱글 플레이인데 들어온 상황이면 즉시 이탈
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        PhotonNetwork.LoadLevel("Multi Room"); // 이동 할 씬 이름

        Debug.Log("캐릭터 생성");
    }

    public override void OnDisconnected(DisconnectCause cause) // 네트워크가 끊겼을 때 호출되는 함수
    {
        Debug.LogWarning($"서버 연결이 끊어졌습니다. : {cause}");
    }
}
