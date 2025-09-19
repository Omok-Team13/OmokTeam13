using UnityEngine;

public class NameBillboard : MonoBehaviour
{
    Transform mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(0, 0, 0);
        //transform.LookAt(mainCamera);
    }
}
