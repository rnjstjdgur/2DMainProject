using UnityEngine;

public class GameOverPopup : DaniTechUIBase
{
    [Header("버튼")]
    [SerializeField] private DaniTechUIButton Btn_GoToRobby;
    [SerializeField] private DaniTechUIButton Btn_Retry;

    private void OnEnable()
    {
        Btn_GoToRobby.BindOnClickButtonEvent(OnClick_GoToRobby);
        Btn_Retry.BindOnClickButtonEvent(OnClick_Retry);
    }

    public void OnClick_GoToRobby()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.GameOverPopup);
        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.RobbyUI);
    }

    public void OnClick_Retry()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.GameOverPopup);
        DaniTechGameManager.Inst.StartGame();
    }
}
