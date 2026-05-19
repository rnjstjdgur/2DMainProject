using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameBookSlotUI : MonoBehaviour
{
    [Header("기본 정보")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private GameObject GObj_Selected; // 활성 비활성화 기능으로만 사용할거라서 이미지가 아니라 게임오브젝트로 함
    [SerializeField] private DaniTechUIButton Button_SlotClick;

    private event Action<string> _onClickSlot;

    private string _slotDataId;

    public string GetSlotDataId()
    {
        return _slotDataId;
    }

    private void OnEnable()
    {
        Button_SlotClick.BindOnClickButtonEvent(OnClick_GameBookSlot);
    }

    public void OnClick_GameBookSlot()
    {
        // 자식이 눌러져서 부모한테 알림
        _onClickSlot?.Invoke(_slotDataId);
    }

    private void OnDisable()
    {
        _onClickSlot = null;
    }

    public void InitSlot(string dataId, Action<string> onClickCallback/*TableType*/)    //TODO
    {
        var itemData = DaniTechGameDataManager.Instance.GetDNItemData(dataId);
        if (itemData == null) return;

        Text_MainName.text = itemData.Name;

        string iconPath = itemData.IconPath;
        if (string.IsNullOrEmpty(iconPath) == true) return;

        // 이건 잘 만들어 둔거니까 그냥 사용하기 <Image에 아이콘, sprite리소스 불러와서 표기해줄때>
        DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, iconPath).Forget();
        

        _slotDataId = dataId;

        _onClickSlot += onClickCallback;

        // Text_MainName.text = 
        // TODO 슬롯 로드가 들어갈 예정
        // Image_MainIcon.sprite = 
    }

    public void SetSelectedUI(bool isSelect)
    {
        GObj_Selected.SetActive(isSelect);
    }
}
