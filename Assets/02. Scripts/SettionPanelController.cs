using UnityEngine;

public class SettingPanelController : MonoBehaviour
{
    public GameObject settingPanel;

    public void ClosePanel()
    {
        settingPanel.SetActive(false);
    }
}