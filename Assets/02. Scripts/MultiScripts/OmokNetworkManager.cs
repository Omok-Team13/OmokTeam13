using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class OmokNetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // 가장 먼저 마스터 서버에 접속 시도
        Debug.Log("Connecting to Master...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // 마스터 서버 접속 성공 시 자동 호출되는 콜백 함수
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master!");
        // 로비에 접속하여 다른 룸 목록을 보거나, 룸에 참가할 수 있게 됨
        PhotonNetwork.JoinLobby();
    }

    // 로비 접속 성공 시 자동 호출
    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby!");
        // 여기에 "랜덤 매칭 시작" 버튼을 누르면 호출될 함수를 연결합니다.
    }

    // "게임 시작" 버튼에 연결할 함수
    public void JoinRandomRoom()
    {
        Debug.Log("Trying to join a random room...");
        PhotonNetwork.JoinRandomRoom();
    }

    // 랜덤 룸 참가가 실패했을 때 (참가할 방이 없을 때) 자동 호출
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("No rooms to join, creating a new one...");
        // 최대 2명까지 들어올 수 있는 방을 만듭니다.
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(null, roomOptions); // 이름이 null이면 서버가 랜덤 생성
    }

    // 룸에 성공적으로 들어갔을 때 (내가 만들었든, 남의 방에 들어갔든) 자동 호출
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room successfully!");
        Debug.Log("Room Name: " + PhotonNetwork.CurrentRoom.Name);

        // 여기에 게임 씬을 로드하는 코드를 추가합니다.
        // 중요: 마스터 클라이언트(방장)만 씬을 로드해야 모두가 동기화됩니다.
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene"); // 오목 게임이 진행될 씬 이름
        }
    }
}