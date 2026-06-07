using UnityEngine;

public class SettingPopup : DaniTechUIBase
{
    [Header("버튼")]
    [SerializeField] private DaniTechUIButton Button_ClosePopup;
    [SerializeField] private DaniTechUIButton Button_GameBook;
    [SerializeField] private DaniTechUIButton Button_Magic;
    [SerializeField] private DaniTechUIButton Button_Tutorial;

    [Header("마법종류 버튼")]
    [SerializeField] private DaniTechUIButton Button_MagicArrow;
    [SerializeField] private DaniTechUIButton Button_Ice;
    [SerializeField] private DaniTechUIButton Button_Fire;
    [SerializeField] private DaniTechUIButton Button_Lightning;
    [SerializeField] private DaniTechUIButton Button_Back;

    [Header("애니메이터")]
    [SerializeField] private Animator Animator_Animation;

    [Header("레이아웃")]
    [SerializeField] private GameObject Layout_MagicButton;
    [SerializeField] private GameObject Layout_Button;

    private void OnEnable()
    {
        Button_ClosePopup.BindOnClickButtonEvent(OnClick_ClosePopup);
        Button_GameBook.BindOnClickButtonEvent(OnClick_OpenGameBook);
        Button_Magic.BindOnClickButtonEvent(OnClick_ShowMagicButton);
        Button_Tutorial.BindOnClickButtonEvent(OnClick_OpenTutorialSequencer);
        Button_MagicArrow.BindOnClickButtonEvent(OnClick_MagicArrowSequencer);
        Button_Ice.BindOnClickButtonEvent(OnClick_IceSequencer);
        Button_Fire.BindOnClickButtonEvent(OnClick_FireSequencer);
        Button_Lightning.BindOnClickButtonEvent(OnClick_LightningSequencer);
        Button_Back.BindOnClickButtonEvent(OnClick_BackToSettingButton);
    }

    // 세팅팝업 버튼 이벤트 =========================================================

    private void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.SettingPopup);
    }

    private void OnClick_OpenGameBook()
    {
        DaniTechUIManager.Instance.OpenContentUI(DaniTechUIType.GameBookUI);
    }

    private void OnClick_ShowMagicButton()
    {
        Layout_Button.SetActive(false);
        Layout_MagicButton.SetActive(true);
    }

    private void OnClick_OpenTutorialSequencer()
    {

    }

    // 마법 시연 영상 버튼 이벤트 =============================================

    private void OnClick_MagicArrowSequencer()
    {
        
    }

    private void OnClick_IceSequencer()
    {
       
    }

    private void OnClick_FireSequencer()
    {
        
    }

    private void OnClick_LightningSequencer()
    {
    }

    private void OnClick_BackToSettingButton()
    {
        Layout_MagicButton.SetActive(false);
        Layout_Button.SetActive(true);
    }

    // 영상 재생관련 ============================================

}
