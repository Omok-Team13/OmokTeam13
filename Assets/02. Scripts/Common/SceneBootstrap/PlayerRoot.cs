using UnityEngine;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

public class PlayerRoot : MonoBehaviour, IPunInstantiateMagicCallback
{
    public static PlayerRoot Local; // 싱글플레이 임시
    public Transform Head;

    PhotonView pv;
    ClothesManager clothes;

    void Awake()
    {
#if PHOTON_UNITY_NETWORKING
        var pv = GetComponent<PhotonView>();
        clothes = GetComponentInChildren<ClothesManager>(true);

        bool multiplayer = GameSession.IsMultiplayer;
        bool hasPhoton = pv != null;
        bool mine = hasPhoton && pv.IsMine;

        if (!multiplayer || !hasPhoton) // 싱글플레이
        {
            Local = this; // 내 인스턴스 기록
            DontDestroyOnLoad(gameObject);
        }
        else // 멀티플레이
        {
            if (mine) Local = this; // 내 것만 Local로
        }
#else
        // Photon 없는 빌드(=싱글)도 동일 동작
        Local = this;
        DontDestroyOnLoad(gameObject);
#endif 
    }

    // 각 오브젝트가 받은 InstantiateData로 해당 플레이어의 옷을 입힘
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        var data = info.photonView?.InstantiationData; // object[]
        if (clothes == null || data == null || data.Length == 0) return;

        foreach (var o in data)
        {
            var id = o as string;
            if (!string.IsNullOrEmpty(id))
                clothes.EquipById(id); // 카탈로그 통해 슬롯별로 입힘
        }
    }
}

public static class PlayerLocator
{
    public static PlayerRoot GetLocalPlayer() => PlayerRoot.Local;
}
