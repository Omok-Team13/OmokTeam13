using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 오목판의 한 칸을 나타내며, Unity UI 시스템에 맞게 설계되었습니다.
/// 이 클래스는 돌과 금수 표시의 시각적 표현을 관리하고, 사용자 클릭을 처리합니다.
/// </summary>
public class Cell_Omok : MonoBehaviour
{
    // 이 셀의 그리드 좌표입니다.
    public int x, y;

    // 셀 프리팹에 연결된 UI 컴포넌트들에 대한 참조입니다.
    [SerializeField] private Button button;
    [SerializeField] private Image stoneImage;
    [SerializeField] private Image forbiddenMark;

    /// <summary>
    /// 셀에 돌 스프라이트를 설정합니다.
    /// </summary>
    /// <param name="sprite">돌(흑돌 또는 백돌)의 스프라이트, 돌을 없애려면 null을 전달합니다.</param>
    public void SetStone(Sprite sprite)
    {
        if (sprite != null)
        {
            stoneImage.sprite = sprite;
            stoneImage.color = Color.white;
            // 돌이 놓인 칸은 다시 클릭할 수 없도록 비활성화합니다.
            button.interactable = false;
        }
        else
        {
            stoneImage.sprite = null;
            stoneImage.color = Color.clear;
            // 셀을 다시 클릭 가능한 상태로 재설정합니다.
            button.interactable = true;
        }
    }

    /// <summary>
    /// 셀에 금수 표시를 보이거나 숨깁니다.
    /// </summary>
    /// <param name="isForbidden">금수 표시를 보이려면 true, 숨기려면 false.</param>
    /// <param name="forbiddenSprite">금수 표시를 위한 스프라이트.</param>
    public void SetForbidden(bool isForbidden, Sprite forbiddenSprite = null)
    {
        if (forbiddenMark != null)
        {
            if (isForbidden)
            {
                forbiddenMark.sprite = forbiddenSprite;
                forbiddenMark.enabled = true;
            }
            else
            {
                forbiddenMark.sprite = null;
                forbiddenMark.enabled = false;
            }
        }
        // 금수 칸은 클릭할 수 없도록 비활성화합니다.
        button.interactable = !isForbidden;
    }

    /// <summary>
    /// 셀의 좌표와 클릭 이벤트 리스너를 설정합니다.
    /// 이 메서드는 그리드가 초기화될 때 한 번 호출되어야 합니다.
    /// </summary>
    /// <param name="x">x 좌표 (열).</param>
    /// <param name="y">y 좌표 (행).</param>
    /// <param name="onCellClicked">이 셀이 클릭되었을 때 호출할 액션입니다.</param>
    public void SetUp(int x, int y, Action<int, int> onCellClicked)
    {
        this.x = x;
        this.y = y;
        // 버튼에 클릭 리스너를 할당합니다.
        button.onClick.AddListener(() => onCellClicked(x, y));

        SetStone(null);
        SetForbidden(false);
    }
}
