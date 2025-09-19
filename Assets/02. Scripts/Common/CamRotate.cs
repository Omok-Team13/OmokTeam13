using UnityEngine;

public class CamRotate : MonoBehaviour
{
    // 코드 담당자 : 최은주 
    /// <summary>
    /// 카메라에 넣으면 화면 돌아가는 코드 
    /// </summary>
    /// 

    enum CursorLockState { Lock, Confine, None };
    CursorLockState cursorMode;
    
    public float rotSpeed = 200f;

    public float mx = 0;
    public float my = 0;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!StateLogic.Instance.isOmok)
            CamRotation();
    }

    void CamRotation()
    {        

        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");

        mx += mouse_X * rotSpeed * Time.deltaTime;
        my += mouse_Y * rotSpeed * Time.deltaTime;

        my = Mathf.Clamp(my, -90f, 90f); //-90과 90 사이로 회전값 지정

        transform.eulerAngles = new Vector3(-my, mx, 0); //값 회전하는 곳, 적용 
        //transform.rotation <- Quaternion 값 
    }

    public void MouseLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
}
