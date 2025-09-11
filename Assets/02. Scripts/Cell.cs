using System;
using UnityEngine;
using UnityEngine.UI;

public class Cell_Omok : MonoBehaviour
{
    public int x, y;

    [SerializeField] private Button button;
    [SerializeField] private Image stoneImage;
    [SerializeField] private Image forbiddenMark;

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

    // **������ �κ�: �ݼ� ��������Ʈ ���� �߰�**
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
        button.interactable = !isForbidden;
    }

    public void SetUp(int x, int y, Action<int, int> onCellClicked)
    {
        this.x = x;
        this.y = y;
        button.onClick.AddListener(() => onCellClicked(x, y));
        SetStone(null);
        SetForbidden(false);
    }
}