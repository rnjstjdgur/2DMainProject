using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurrentMagicUI : DaniTechUIBase
{
    [Header("프리팹")]
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private GameObject Prefab_LevelSlot;

    [Header("버튼")]
    [SerializeField] private DaniTechUIButton Button_CloseUI;

    [Header("슬롯 리스트 영역")]
    [SerializeField] private Transform Transform_SlotRoot;

    private Dictionary<string, CurrentMagicSlotUI> _slotList = new Dictionary<string, CurrentMagicSlotUI>();

    private void OnEnable()
    {
        // 이 UI가 열릴때 스스로, 기본적으로 아이템 도감 안에 있는 모든 데이터를 불러온다
        foreach (var key in DaniTechGameObjectManager.Inst.GetSkillList().Keys)
        {
            CreateCurrentMagicSlot(key);
        }
        Button_CloseUI.BindOnClickButtonEvent(OnClick_CloseCurrentMagicUI);
    }

    public void OnClick_CloseCurrentMagicUI()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.CurrentMagicUI);
    }

    private void OnDisable()
    {
        if (_slotList.Count > 0)
        {
            foreach (var slotKv in _slotList)
            {
                var slot = slotKv.Value;
                DestroyImmediate(slot.gameObject);
            }

            _slotList.Clear();
        }
    }

    private void CreateCurrentMagicSlot(string dataId)
    {
        var gObj = Instantiate(Prefab_Slot, Transform_SlotRoot);
        if (gObj == null) return;

        var slotComponent = gObj.GetComponent<CurrentMagicSlotUI>();
        if (slotComponent == null) return;

        slotComponent.InitSlot(dataId);
        _slotList.Add(dataId, slotComponent);

    }
}
