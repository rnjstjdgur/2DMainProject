using UnityEngine;
using UnityEngine.UI;

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

    [Header("애니메이터")]
    [SerializeField] private Animator Animator_Tutorial;

    [Header("레이아웃")]
    [SerializeField] private GameObject Layout_MagicButton;
    [SerializeField] private GameObject Layout_Button;

    [Header("자막")]
    [SerializeField] private Text Text_Subtitle;

    [Header("커튼용 이미지")]
    [SerializeField] private Image Image_Curtain;

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
        SetLayoutActive(Layout_MagicButton, false);
        Text_Subtitle.gameObject.SetActive(false);
        Image_Curtain.gameObject.SetActive(true);
    }

    // 세팅팝업 버튼 이벤트 =========================================================

    private void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.ClosePopupUI(DaniTechUIType.SettingPopup);
    }

    private void OnClick_ShowMagicButton()
    {
        Image_Curtain.gameObject.SetActive(true);
        SetLayoutActive(Layout_MagicButton, true);
        SetLayoutActive(Layout_Button, false);
    }

    private void OnClick_OpenTutorialSequencer()
    {
        StopAllSequencer();
        Animator_Tutorial.gameObject.SetActive(true);
        Text_Subtitle.gameObject.SetActive(true);
    }

    // 마법 시연 영상 버튼 이벤트 =============================================

    private void OnClick_MagicArrowSequencer()
    {
        StopAllSequencer();
        Sequencer_MagicArrow.gameObject.SetActive(true);
    }

    private void OnClick_IceSequencer()
    {
        StopAllSequencer();
        Sequencer_Ice.gameObject.SetActive(true);
    }

    private void OnClick_FireSequencer()
    {
        StopAllSequencer();
        Sequencer_Fire.gameObject.SetActive(true);
    }

    private void OnClick_LightningSequencer()
    {
        StopAllSequencer();
        Sequencer_Lightning.gameObject.SetActive(true);
    }

    private void OnClick_BackToSettingButton()
    {
        SetLayoutActive(Layout_MagicButton, false);
        SetLayoutActive(Layout_Button, true);
        Image_Curtain.gameObject.SetActive(true);
    }

    // 레이아웃 관련 ============================================
    private void SetLayoutActive(GameObject layout, bool isActive)
    {
        // 끄는 대신 크기를 0으로 줄여서 눈에 안 보이게 합니다.
        layout.transform.localScale = isActive ? Vector3.one : Vector3.zero;
    }

    // 영상 재생관련 ============================================

    private void StopAllSequencer()
    {
        Sequencer_MagicArrow.gameObject.SetActive(false);
        Sequencer_Ice.gameObject.SetActive(false);
        Sequencer_Fire.gameObject.SetActive(false);
        Sequencer_Lightning.gameObject.SetActive(false);
        Animator_Tutorial.gameObject.SetActive(false);
        Image_Curtain.gameObject.SetActive(false);
        Text_Subtitle.gameObject.SetActive(false);
    }

    // 애니메이션 함수 =========================================

}
