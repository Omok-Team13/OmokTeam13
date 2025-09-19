using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterTransformManager : MonoBehaviour
{
    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StartCoroutine(Setup());
    }

    public IEnumerator Setup()
    {
        while (PlayerLocator.GetLocalPlayer() == null) yield return null; //네트워크 스폰 대기
        yield return null;  // 씬 객체 초기화/찾기 안정화

        var cam = Camera.main;
        if (!cam) yield break;

        var localPlayer = PlayerLocator.GetLocalPlayer();
        if (!localPlayer) yield break;

        // 플레이어 위치 초기화
        localPlayer.transform.SetPositionAndRotation(transform.position, transform.rotation);

        //메인카메라를 로컬플레이어의 헤드에 붙이기        
        //{
        //    Transform mount = localPlayer.Head != null ? localPlayer.Head : localPlayer.transform;
        //    cam.transform.SetParent(mount, worldPositionStays: false);
        //    cam.transform.localPosition = Vector3.zero;
        //    cam.transform.localRotation = Quaternion.identity;
        //}
    }
}