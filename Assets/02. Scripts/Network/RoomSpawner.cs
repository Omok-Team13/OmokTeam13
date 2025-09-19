using System.Collections;
using Photon.Pun;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    public string playerName = "IntroCharacter";

    void Start()
    {
        if (!GameSession.IsMultiplayer) // 싱글플레이에서는 작동되지 않도록
        {
            enabled = false;
            return;
        }

        StartCoroutine(SpawnWhenReady());
    }

    IEnumerator SpawnWhenReady()
    {
        // Pun 준비 대기
        while (!PhotonNetwork.IsConnectedAndReady || !PhotonNetwork.InRoom)
            yield return null;

        // 재생성 방지 (이미 네트워크상에 존재하는데 또 생성될까봐)
        if (PhotonNetwork.LocalPlayer.TagObject != null)
            yield break;

        // 캐릭터를 Room에 생성 (이때 위치는 임시용이고 실제로는 CharacterTransformManager가 위치 초기화)
        var data = (object[])(GameSession.OutfitIds ?? new string[0]); // 착장 정보 갖고오기
        var go = PhotonNetwork.Instantiate(playerName, Vector3.zero, Quaternion.identity, 0, data);

        Debug.Log($"[Spawn] outfitIds=({string.Join(",", GameSession.OutfitIds ?? new string[0])})");

        // 내 레퍼런스를 LocalPlayer에 저장
        PhotonNetwork.LocalPlayer.TagObject = go;
    }
}
