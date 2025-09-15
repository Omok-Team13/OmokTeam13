using System;
using UnityEngine;
using UnityEngine.UI;

// --- 파일 이름을 Cell_Omok.cs로 맞춰주세요 ---
public class Cell_Omok : MonoBehaviour
{
    // 이 셀의 그리드 좌표
    public int x, y;

    // --- 유니티 에디터에서 연결할 UI 컴포넌트들 ---
    [SerializeField] private Button button;       // 클릭을 감지할 버튼
    [SerializeField] private Image stoneImage;     // 흑돌 또는 백돌 이미지를 표시할 Image
    [SerializeField] private Image markImage;      // '선택됨'을 표시할 Image (예: 테두리, 점)

    /// <summary>
    /// 셀을 초기화하고 클릭 이벤트를 설정합니다.
    /// BoardController에서 게임 시작 시 호출됩니다.
    /// </summary>
    public void SetUp(int x, int y, Action<int, int> onCellClicked)
    {
        this.x = x;
        this.y = y;

        // 버튼에 등록된 기존 이벤트를 모두 지우고 새로 추가합니다.
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onCellClicked(x, y));

        SetStone(null);       // 돌 이미지를 초기화합니다.
        SetMark(false, null); // 선택 표시를 숨깁니다.
    }

    /// <summary>
    /// 이 셀에 돌 이미지를 설정합니다.
    /// </summary>
    /// <param name="sprite">표시할 돌의 스프라이트. null이면 빈 칸으로 만듭니다.</param>
    public void SetStone(Sprite sprite)
    {
        if (stoneImage == null) return; // 안전 코드

        if (sprite != null)
        {
            stoneImage.sprite = sprite;
            stoneImage.color = Color.white;
            button.interactable = false; // 돌이 놓이면 더 이상 클릭할 수 없습니다.
        }
        else
        {
            stoneImage.sprite = null;
            stoneImage.color = Color.clear;
            button.interactable = true; // 빈 칸은 다시 클릭할 수 있습니다.
        }
    }

    /// <summary>
    /// '선택됨' 표시를 켜거나 끕니다.
    /// BoardController_Omok에서 이 함수를 호출하여 선택 상태를 시각적으로 보여줍니다.
    /// </summary>
    /// <param name="isMarked">표시를 켜려면 true, 끄려면 false.</param>
    /// <param name="sprite">표시할 이미지의 스프라이트.</param>
    public void SetMark(bool isMarked, Sprite sprite)
    {
        if (markImage == null) return; // 안전 코드

        if (isMarked)
        {
            markImage.sprite = sprite;
            markImage.enabled = true; // 이미지를 활성화하여 보여줍니다.
        }
        else
        {
            markImage.enabled = false; // 이미지를 비활성화하여 숨깁니다.
        }
    }
}

