using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera omokCamera; //오목 시작 시 전환해주는 카메라
    [SerializeField] Camera OmokWinnerCam;    
    public Camera playerCamera; //플레이어 머리에 달린 카메라     
    public Camera BoxingWinCam;
    public TextMeshPro playerName;

    public enum currCamState { EnterBoxing, EnterOmok, EndOmok, Intro, OmokWinner, BoxingWinner }
    public currCamState currCam;

    CamRotate camRotate;

    private void Awake()
    {
        omokCamera.enabled = false;
    }

    IEnumerator InitCamera()
    {
        yield return null;
         
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = player.GetComponentInChildren<Camera>();
        playerName = player.Find("Name").GetComponent<TextMeshPro>();
        BoxingWinCam = player.Find("Boxing Win").GetComponent<Camera>();
       
        camRotate = playerCamera.GetComponent<CamRotate>();
        playerCamera.GetComponent<CamRotate>().enabled = true;

        BoxingWinCam.enabled = false;
        playerCamera.enabled = true;
        mainCamera.enabled = false;                
       
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
                playerName.enabled = false;
                break;
            case currCamState.EnterBoxing: //복싱 카메라 키기
                playerName.enabled = false;
                mainCamera.enabled = true;                
                StartCoroutine(turnOffMain());                                                                                      
                omokCamera.enabled = false;              
                break;
            case currCamState.EndOmok: //플레이어 카메라 키기 
                playerCamera.enabled = true;
                omokCamera.enabled = false;
                mainCamera.enabled = false;                
                playerName.enabled = true;
                break;
            case currCamState.Intro: //플레이어 카메라키기
                mainCamera.enabled = false;
                omokCamera.enabled = false;
                playerCamera.enabled = false;                
                break;
            case currCamState.BoxingWinner:
                BoxingWinCam.enabled = true;
                mainCamera.enabled = false;
                omokCamera.enabled = false;
                playerCamera.enabled = false;
                OmokWinnerCam.enabled = false;
                break;
            case currCamState.OmokWinner:
                OmokWinnerCam.enabled = true;
                BoxingWinCam.enabled = false;
                mainCamera.enabled = false;
                omokCamera.enabled = false;
                playerCamera.enabled = false;
                break;            
        }
    }

    public void SwitchCamera(currCamState newState)
    {
        currCam = newState;
        CurrentCamPos();
    }
    IEnumerator turnOffMain()
    {        
        yield return new WaitForSeconds(3.9f); //오목판 엎는 애니메이션 기다리기
        mainCamera.enabled = false;
        //boxingCamera.enabled = true;
        playerCamera.enabled = true;
    }
}

