using UnityEngine;

public class SettingPopup : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_ClosePopup;

    private void OnEnable()
    {
        Button_ClosePopup.BindOnClickButtonEvent(OnClick_ClosePopup);
    }

    public void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.SettingPopup);
    }
}
