using UnityEngine;

public class AskEndGameUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_Yes;
    [SerializeField] private DaniTechUIButton Button_No;

    private void OnEnable()
    {
        Button_Yes.BindOnClickButtonEvent(OnClick_EndGame);
        Button_No.BindOnClickButtonEvent(OnClick_ClosePopup);
    }

    private void OnClick_EndGame()
    {
        DaniTechUIManager.Instance.CloseBackContentUI(DaniTechUIType.EscUI);
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.AskEndGameUI);
        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.RobbyUI);
    }

    private void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.AskEndGameUI);
    }
}
