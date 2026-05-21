using UnityEngine;

public class RobbyUI : DaniTechUIBase
{
    [SerializeField] private DaniTechUIButton Button_GameStart;

    private void OnEnable()
    {
        Button_GameStart.BindOnClickButtonEvent(OnClick_GameStart);
    }

    public void OnClick_GameStart()
    {
        DaniTechGameManager.Inst.StartGame();
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.RobbyUI);
    }

    public void OnClick_GameQuit()
    {
        DaniTechGameManager.Inst.SaveAndEndGame();
    }
}
