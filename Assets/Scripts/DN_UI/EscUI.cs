using UnityEngine;

public class ESCUI : DaniTechUIBase
{
    [Header("버튼")]
    [SerializeField] private DaniTechUIButton Button_GameBook;
    [SerializeField] private DaniTechUIButton Button_CurrentMagic;
    [SerializeField] private DaniTechUIButton Button_EndGame;
    [SerializeField] private DaniTechUIButton Button_Close;
    private void OnEnable()
    {
        Button_GameBook.BindOnClickButtonEvent(OnClick_OpenGameBook);
        Button_CurrentMagic.BindOnClickButtonEvent(OnClick_OpenCurrentMagic);
        Button_EndGame.BindOnClickButtonEvent(OnClick_EndGame);
        Button_Close.BindOnClickButtonEvent(OnClick_CloseUi);
        TimeManager.instance.TimeStop();
    }

    private void OnClick_OpenGameBook()
    {
        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.GameBookUI);
    }

    private void OnClick_OpenCurrentMagic()
    {

    }

    private void OnClick_EndGame()
    {
        DaniTechUIManager.Instance.OpenPopupUI(DaniTechUIType.AskEndGameUI);
    }

    private void OnClick_CloseUi()
    {
        TimeManager.instance.TimeStart();
        DaniTechUIManager.Instance.CloseBackContentUI(DaniTechUIType.EscUI);
    }
}
