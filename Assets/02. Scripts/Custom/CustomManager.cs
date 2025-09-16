using Unity.Cinemachine;
using UnityEngine;

public class CustomManager : MonoBehaviour
{
    //코드 담당자 : 최은주

    [SerializeField] GameObject customUI;
    [SerializeField] GameObject basicUI;

    [SerializeField] GameObject basicEnv;
    [SerializeField] GameObject customEnv;

    [SerializeField] CinemachineCamera basicCamera;
    [SerializeField] CinemachineCamera customCamera;


    public void CustomON()
    {
        customUI.SetActive(true);
        basicUI.SetActive(false);

        basicEnv.SetActive(false);
        customEnv.SetActive(true);

        basicCamera.gameObject.SetActive(false);
        customCamera.gameObject.SetActive(true);
    }

    public void CustomOff()
    {
        customUI.SetActive(false);
        basicUI.SetActive(true);

        basicEnv.SetActive(true);
        customEnv.SetActive(false);

        basicCamera.gameObject.SetActive(true);
        customCamera.gameObject.SetActive(false);
    }

}
