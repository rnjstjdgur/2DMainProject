using UnityEngine;

public class SettingPopup : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_Setting;

    private void OnEnable()
    {
        Button_Setting.BindOnClickButtonEvent(OnClick_Setting);
    }

    public void OnClick_Setting()
    {
        DaniTechUIManager.Instance.OpenPopupUI(DaniTechUIType.SettingPopup);
    }
}
