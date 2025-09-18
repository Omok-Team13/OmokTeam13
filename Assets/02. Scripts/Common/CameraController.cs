using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera omokCamera; //오목 시작 시 전환해주는 카메라
    public Camera playerCamera; //플레이어 머리에 달린 카메라 
    public Camera boxingCamera; //플레이어한테 달린 복싱뷰 카메라    

    public enum currCamState { EnterBoxing, EnterOmok, EndOmok, Intro }
    public currCamState currCam;

    private void Awake()
    {
        omokCamera.enabled = false;
    }

    IEnumerator InitCamera()
    {
        yield return null;

        var player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = player.Find("Play").GetComponent<Camera>();
        boxingCamera = player.Find("Boxing").GetComponent<Camera>();

        playerCamera.GetComponent<CamRotate>().enabled = true;
        playerCamera.enabled = true;
        mainCamera.enabled = false;        
        boxingCamera.enabled = false;
       
        currCam = currCamState.EndOmok;
    }

    private void Start()
    {       
        StartCoroutine(InitCamera());
        
    }

    public void CurrentCamPos()
    {
        switch (currCam)
        {
            case currCamState.EnterOmok: //오목 카메라 키기
                omokCamera.enabled = true;
                mainCamera.enabled = false;
                playerCamera.enabled = false;
                boxingCamera.enabled = false;

                break;
            case currCamState.EnterBoxing: //복싱 카메라 키기
                boxingCamera.enabled = true;
                playerCamera.enabled = false;
                mainCamera.enabled = false;
                omokCamera.enabled = false;                
                break;
            case currCamState.EndOmok: //플레이어 카메라 키기 
                playerCamera.enabled = true;
                omokCamera.enabled = false;
                mainCamera.enabled = false;
                boxingCamera.enabled = false;
                break;
            case currCamState.Intro: //플레이어 카메라키기
                mainCamera.enabled = false;
                omokCamera.enabled = false;
                playerCamera.enabled = false;
                boxingCamera.enabled = false;
                break;
        }
    }

    public void SwitchCamera(currCamState newState)
    {
        currCam = newState;
        CurrentCamPos();
    }
}

