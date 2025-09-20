using UnityEngine;

public class SettionButtonUI: MonoBehaviour
{
    public GameObject settingPanel;

    // 버튼에서 이 함수를 연결하면 패널 On/Off 토글
    public void ToggleSettingPanel()
    {
        bool isActive = settingPanel.activeSelf;
        settingPanel.SetActive(!isActive);
    }
}