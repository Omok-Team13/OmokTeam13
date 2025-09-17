using UnityEngine;

public class PlayerRoot : MonoBehaviour
{
    public static PlayerRoot Local; // 싱글플레이 임시
    public Transform Head;

    void Awake()
    {
        Local = this; // 내 플레이어 인스턴스 기록
        DontDestroyOnLoad(gameObject);
    }
}

public static class PlayerLocator
{
    public static PlayerRoot GetLocalPlayer() => PlayerRoot.Local;
}
