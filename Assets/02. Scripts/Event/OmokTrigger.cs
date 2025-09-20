using Controller;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class OmokTrigger : MonoBehaviour
{
    // 유니티 에디터에서 연결할 오목 보드 UI
    [SerializeField] GameObject BoxingCinema;
    [SerializeField] Camera mainCamera;
    
    public GameObject omokUi;
    public Button sitButton;
    public Button standButton;

    public Button startButton;
    public Transform chair;

    public Button boxingButton;

    GameObject player;
    CamRotate camRotate;
    CharacterController cc;
    Vector3 center;

    Camera playerCamera;
    // 유니티 에디터에서 연결할 오목 게임 매니저
    public GameObject omokManager;
    
    private void Awake()
    {
           
        player = GameObject.FindWithTag("Player");
        
        var playerCam = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = player.GetComponentInChildren<Camera>();        

        camRotate = mainCamera.GetComponent<CamRotate>();

        cc = player.GetComponent<CharacterController>();
        center = cc.center;        

        sitButton.onClick.AddListener(() =>
        {
            player.transform.rotation = Quaternion.identity;
            //player.gameObject.GetComponent<CharacterController>().enabled = false;
            center.y = 0.75f;
            cc.center = center;
            player.transform.position = chair.position;
            player.gameObject.GetComponent<Animator>().SetTrigger("Sit");

            player.gameObject.GetComponent<CharacterMover>().enabled = false;
            startButton.gameObject.SetActive(true);
            this.sitButton.gameObject.SetActive(false);
            standButton.gameObject.SetActive(true);
        });

        boxingButton.onClick.AddListener(() =>
        {
            center.y = 1.14f;
            cc.center = center;
            StartCoroutine(waitcc());                        
            player.gameObject.GetComponent<CharacterController>().enabled = true;
            player.gameObject.GetComponent<CharacterMover>().enabled = true;           
            StateLogic.Instance.SetState(StateLogic.GameState.EnterBoxing);
            StateLogic.Instance.turnOffBattleButton(1);
            this.boxingButton.gameObject.SetActive(false);

        });

        startButton.onClick.AddListener(() => //상태 오목 enter로
        {
            center.y = 0.75f;
            cc.center = center;            
            StateLogic.Instance.RoundScore(1, false, false); //라운드 값, 무조건 상태 들어가기 전에 전달            
            StateLogic.Instance.SetState(StateLogic.GameState.EnterOmok);
            this.startButton.gameObject.SetActive(false);
            standButton.gameObject.SetActive(false);
        });
        standButton.onClick.AddListener(() =>
        {            
            center.y = 0.95f;
            cc.center = center;
            //player.gameObject.GetComponent<CharacterController>().enabled = true;
            GameObject.FindWithTag("Player").gameObject.GetComponent<Animator>().SetTrigger("Stand");
            player.gameObject.GetComponent<CharacterMover>().enabled = true;
            sitButton.gameObject.SetActive(true);
            standButton.gameObject.SetActive(false);
            startButton.gameObject.SetActive(false);
        });
    }

    // 다른 Collider가 이 트리거 영역으로 들어왔을 때 자동으로 호출됩니다.
    private void OnTriggerEnter(Collider other)
    {
        Cursor.lockState = CursorLockMode.None;
        // 들어온 오브젝트의 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {          
            sitButton.gameObject.SetActive(true);           

            Debug.Log("플레이어가 오목존에 진입했습니다.");        

            // 연결된 오목 매니저가 있다면 활성화시킵니다.
            if (omokManager != null)
            {
                omokManager.SetActive(true);
            }                 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            sitButton.gameObject.SetActive(false);           
            startButton.gameObject.SetActive(false);
        }

    }

    IEnumerator waitcc() 
    {               
        camRotate.MouseLock();
        player.transform.rotation = Quaternion.identity;                

        //center.y = 1.14f;
        //cc.center = center;
        yield return new WaitForSeconds(1f);
        //center.y = 0.95f;
        //cc.center = center;
    }
}