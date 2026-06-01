using UnityEngine;

public class RobbyUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_GameStart;
    [SerializeField] private DaniTechUIButton Button_Setting;

    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
        Button_Setting.BindOnClickButtonEvent(OnClick_OpenSettingPopup);
    }

    public void OnClick_GameStart()
    {
        DaniTechGameManager.Inst.StartGame();
        DaniTechGameManager.Inst.RestartGame();
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.RobbyUI);
    }

    public void OnClick_GameQuit()
    {
        DaniTechGameManager.Inst.SaveAndEndGame();
    }

    public void OnClick_OpenSettingPopup()
    {
        DaniTechUIManager.Instance.OpenPopupUI(DaniTechUIType.SettingPopup);
    }
}
