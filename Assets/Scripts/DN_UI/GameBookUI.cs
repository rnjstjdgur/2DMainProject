using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

public class GameBookUI : DaniTechUIBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_Slot;

    [Header("디테일 정보 영역")]
    [SerializeField] private Image Image_MainIcon;
    [SerializeField] private Text Text_MainName;
    [SerializeField] private Text Text_Description;

    [SerializeField] private DaniTechUIButton Button_CloseUI;

    //[Header("부가 정보")]
    //[SerializeField] private GameObject Layout_SubInfoSkill; // 그 안에 있는 UI 요소를 직접 하나하나 껐다켰다 하는게 아니라 그 레이아웃의 대표 오브젝트만 껐다 켰다 하는게 압도적으로 편하다

    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot;

    private Dictionary<string, GameBookSlotUI> _slotList = new Dictionary<string, GameBookSlotUI>();

    private void OnEnable()
    {
        // 이 UI가 열릴때 스스로, 기본적으로 아이템 도감 안에 있는 모든 데이터를 불러온다
        ReadItemListAndCreateSlot();
        Button_CloseUI.BindOnClickButtonEvent(OnClick_CloseGameBookUI);
    }

    public void OnClick_CloseGameBookUI()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.GameBookUI);
    }

    private void OnDisable()
    {
        if(_slotList.Count > 0)
        {
            foreach(var slotKv in _slotList)
            {
                var slot = slotKv.Value;
                DestroyImmediate(slot.gameObject);
            }

            _slotList.Clear();
        }
    }

    private void ReadItemListAndCreateSlot()
    {
        var dataList = DaniTechGameDataManager.Instance.ItemDataList;
        foreach (var dataKv in dataList)
        {
            var data = dataKv.Value;
            if (data == null) continue;

            CreateGameBookSlot(data.Id);
        }

        if (_slotList.Count > 0)
        {
            foreach (var slotKv in _slotList)
            {
                var slot = slotKv.Value;
                slot.OnClick_GameBookSlot();
            }
        }
    }

    private void CreateGameBookSlot(string dataId)
    {
        var gObj = Instantiate(Prefab_Slot, Transform_SlotRoot);
        if (gObj == null) return;

        var slotComponent = gObj.GetComponent<GameBookSlotUI>();
        if (slotComponent == null) return;

        slotComponent.InitSlot(dataId, onClickChildSlotSelected);
        _slotList.Add(dataId, slotComponent);

    }

    public void onClickChildSlotSelected(string slotDataId)
    {
        var currentSelectedData = DaniTechGameDataManager.Instance.GetDNItemData(slotDataId);
        if (currentSelectedData == null) return;

        // Image_MainIcon;
        Text_MainName.text = currentSelectedData.Name;
        Text_Description.text = currentSelectedData.Description;
        DaniTechGameUtil.LoadAndSetSpriteImage(Image_MainIcon, currentSelectedData.IconPath).Forget();

        foreach(var slotkv in _slotList)
        {
            var slot = slotkv.Value;
            var dataId = slot.GetSlotDataId();
            slot.SetSelectedUI(slotDataId == dataId);
        }
    }
}
