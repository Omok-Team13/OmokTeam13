using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera omokCamera;
    [SerializeField] CinemachineCamera boxingCamera;
    [SerializeField] CinemachineCamera playerCamera;
    
    public enum currCamState { Boxing, Omok }
    public currCamState currCam;

     public void CurrentCamPos()
    {
        switch (currCam)
        {
            case currCamState.Omok:
                omokCamera.enabled = true;
                //var cam = Camera.main;
                //cam = omokCamera;
                omokCamera.Priority = 100;
                playerCamera.enabled = false;
                boxingCamera.enabled = false;
                playerCamera.Priority = 0;
                boxingCamera.Priority = 0;
                break;
            case currCamState.Boxing:
                boxingCamera.enabled = true;
                //playerCamera.Priority = 10;
                boxingCamera.Priority = 10;
                omokCamera.Priority = 0;
                omokCamera.enabled = false;
                boxingCamera.enabled = false;
                break;
        }
    }

    public void SwitchCamera(currCamState newState)
    {
        currCam = newState;
        CurrentCamPos();
    }

    private void Update()
    {
       if(!StateLogic.Instance.isOmok) //오목 상태가 아닐 때만
        {
            var localPlayer = GameObject.FindWithTag("Player");
            var cam = Camera.main;

            //Transform mount = localPlayer.Head != null ? localPlayer.Head : localPlayer.transform;
            //cam.transform.SetParent(mount, worldPositionStays: false);
            //cam.transform.localPosition = Vector3.zero;
            //cam.transform.localRotation = Quaternion.identity;
        }
        
    }
}
