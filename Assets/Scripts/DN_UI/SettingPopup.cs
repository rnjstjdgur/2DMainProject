using UnityEngine;

public class SettingPopup : DaniTechUIBase
{
    [Header("버튼")]
    [SerializeField] private DaniTechUIButton Button_ClosePopup;
    [SerializeField] private DaniTechUIButton Button_Magic;
    [SerializeField] private DaniTechUIButton Button_Tutorial;

    [Header("마법종류 버튼")]
    [SerializeField] private DaniTechUIButton Button_MagicArrow;
    [SerializeField] private DaniTechUIButton Button_Ice;
    [SerializeField] private DaniTechUIButton Button_Fire;
    [SerializeField] private DaniTechUIButton Button_Lightning;
    [SerializeField] private DaniTechUIButton Button_Back;

    [Header("시퀀서")]
    [SerializeField] private DaniTechUIImageSequencer Sequencer_MagicArrow;
    [SerializeField] private DaniTechUIImageSequencer Sequencer_Ice;
    [SerializeField] private DaniTechUIImageSequencer Sequencer_Fire;
    [SerializeField] private DaniTechUIImageSequencer Sequencer_Lightning;

    [Header("레이아웃")]
    [SerializeField] private GameObject Layout_MagicButton;
    [SerializeField] private GameObject Layout_Button;

    private void OnEnable()
    {
        Button_ClosePopup.BindOnClickButtonEvent(OnClick_ClosePopup);
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

    private void OnClick_ShowMagicButton()
    {
        Layout_Button.SetActive(false);
        Layout_MagicButton.SetActive(true);
    }

    private void OnClick_OpenTutorialSequencer()
    {
        SetVideoNumAndStartVideo(4);
    }

    // 마법 시연 영상 버튼 이벤트 =============================================

    private void OnClick_MagicArrowSequencer()
    {
        SetVideoNumAndStartVideo(0);
    }

    private void OnClick_IceSequencer()
    {
        SetVideoNumAndStartVideo(1);
    }

    private void OnClick_FireSequencer()
    {
        SetVideoNumAndStartVideo(2);
    }

    private void OnClick_LightningSequencer()
    {
        SetVideoNumAndStartVideo(3);
    }

    private void OnClick_BackToSettingButton()
    {
        Layout_MagicButton.SetActive(false);
        Layout_Button.SetActive(true);
    }

    // 영상 재생관련 ============================================

    private void SetVideoNumAndStartVideo(int videoNumber)
    {
        Animator_Animation.SetInteger("videoNum", videoNumber);
    }

}
