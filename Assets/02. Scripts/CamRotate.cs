using UnityEngine;

public class CamRotate : MonoBehaviour
{
    public float rotSpeed = 200f;

    public float mx = 0;
    public float my = 0;

    private void Update()
    {
        float mouse_X = Input.GetAxis("Mouse X");
        float mouse_Y = Input.GetAxis("Mouse Y");

        mx += mouse_X * rotSpeed * Time.deltaTime;
        my += mouse_Y * rotSpeed * Time.deltaTime;

        my = Mathf.Clamp(my, -90f, 90f); //-90과 90 사이로 회전값 지정

        transform.eulerAngles = new Vector3(-my, mx, 0); //값 회전하는 곳, 적용 
        //transform.rotation <- Quaternion 값 
    }
}
