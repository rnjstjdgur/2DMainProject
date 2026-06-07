using UnityEngine;

public class GameClearUI : DaniTechUIBase
{
    [Header("버튼")]
    [SerializeField] private DaniTechUIButton Button_GoToRobby;

    private void OnEnable()
    {
        Button_GoToRobby.BindOnClickButtonEvent(OnClick_GoToRobby);
    }

    private void OnClick_GoToRobby()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.GameClearUI);
        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.RobbyUI);
    }
}
