using System;
using UnityEngine;
using UnityEngine.UI;

public class Cell_Omok : MonoBehaviour
{
    public int x, y;

    [SerializeField] private Button button;
    [SerializeField] private Image stoneImage;
    [SerializeField] private Image markImage;
    // (추가) 금수 표시를 위한 전용 이미지 컴포넌트
    [SerializeField] private Image forbiddenMarkImage;

    public void SetUp(int x, int y, Action<int, int> onCellClicked)
    {
        this.x = x;
        this.y = y;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onCellClicked(x, y));
        SetStone(null);
        SetMark(false, null);
        SetForbidden(false, null); // 시작 시 금수 표시도 숨김
    }

    public void SetStone(Sprite sprite)
    {
        if (sprite != null)
        {
            stoneImage.sprite = sprite;
            stoneImage.color = Color.white;
            button.interactable = false;
        }
        else
        {
            stoneImage.sprite = null;
            stoneImage.color = Color.clear;
            button.interactable = true;
        }
    }

    public void SetMark(bool isMarked, Sprite sprite)
    {
        if (markImage == null) return;
        markImage.enabled = isMarked;
        if (isMarked)
        {
            markImage.sprite = sprite;
        }
    }

    /// <summary>
    /// (추가) 금수 표시 이미지를 켜고 끄는 함수입니다.
    /// </summary>
    public void SetForbidden(bool isForbidden, Sprite sprite)
    {
        if (forbiddenMarkImage == null) return;

        forbiddenMarkImage.enabled = isForbidden;
        if (isForbidden)
        {
            forbiddenMarkImage.sprite = sprite;
        }
    }
}
