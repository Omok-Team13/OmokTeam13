using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class OmokTrigger : MonoBehaviour
{
    // 유니티 에디터에서 연결할 오목 보드 UI
    public CinemachineCamera omokCamera;
    public GameObject omokUi;
    public Button sitButton;
    public Button standButton;

    public Button startButton;
    public Transform chair;

    public Button boxingButton;

    GameObject player;

    // 유니티 에디터에서 연결할 오목 게임 매니저
    public GameObject omokManager;
    
    private void Awake()
    {
        player = GameObject.FindWithTag("Player");

        sitButton.onClick.AddListener(() =>
        {
            player.gameObject.GetComponent<CharacterController>().enabled = false;
            player.transform.position = chair.position;
            player.gameObject.GetComponent<Animator>().SetTrigger("Sit");            

            startButton.gameObject.SetActive(true);
            this.sitButton.gameObject.SetActive(false);
            standButton.gameObject.SetActive(true);
        });

        boxingButton.onClick.AddListener(() =>
        {
            var nextState = GameManage.Instance.GetState(1);
            StateLogic.Instance.SetState(nextState);
            StateLogic.Instance.turnOffBattleButton(1);
            this.boxingButton.gameObject.SetActive(false);
            player.gameObject.GetComponent<CharacterController>().enabled = true;

        });

        startButton.onClick.AddListener(() => //상태 오목 enter로
        {
            var nextState = GameManage.Instance.GetState(0);
            StateLogic.Instance.SetState(nextState);
            StateLogic.Instance.RoundScore(1, false); //라운드 값
            this.startButton.gameObject.SetActive(false);
        });
        standButton.onClick.AddListener(() =>
        {
            player.gameObject.GetComponent<CharacterController>().enabled = true;
            GameObject.FindWithTag("Player").gameObject.GetComponent<Animator>().SetTrigger("Stand");
            sitButton.gameObject.SetActive(true);
            standButton.gameObject.SetActive(false);          
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
            omokCamera.gameObject.SetActive(true);

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
            omokCamera.gameObject.SetActive(false);
            startButton.gameObject.SetActive(false);
        }

    }
}