using UnityEngine;
using UnityEngine.UI;

public class OmokTrigger : MonoBehaviour
{
    // 유니티 에디터에서 연결할 오목 보드 UI
    public GameObject omokBoardUI;
    public Button sitButton;

    // 유니티 에디터에서 연결할 게임 매니저
    public GameObject gameManager;

    

    private void Awake()
    {        
        sitButton.onClick.AddListener(() =>
        {                        
            GameObject.FindWithTag("Player").gameObject.GetComponent<Animator>().SetTrigger("Sit");
        });
    }


    // 다른 Collider가 이 트리거 영역으로 들어왔을 때 자동으로 호출됩니다.
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트의 태그가 "Player"인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log("플레이어가 오목존에 진입했습니다. 게임을 시작합니다!");

            // 연결된 오목 UI가 있다면 활성화시킵니다.
            if (omokBoardUI != null)
            {
                omokBoardUI.SetActive(true);                
            }

            // 연결된 게임 매니저가 있다면 활성화시킵니다.
            if (gameManager != null)
            {
                gameManager.SetActive(true);
            }            

            // (선택) 트리거가 한 번만 작동하게 하려면 아래 줄의 주석을 푸세요.
            // gameObject.SetActive(false); 
        }
    }
}