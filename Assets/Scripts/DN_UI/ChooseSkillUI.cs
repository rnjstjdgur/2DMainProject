using System.Collections.Generic;
using UnityEngine;

public class ChooseSkillUI : DaniTechUIBase
{
    [SerializeField] private GameObject Prefab_Slot;
    [SerializeField] private Transform Transform_UISlotRoot;
    [SerializeField] private DaniTechUIButton Button_CloseSelfAllArea;

    private int _generatedKey = 0;
    private Dictionary<int, ChooseSkillSlotUI> _skillSlotList = new Dictionary<int, ChooseSkillSlotUI>();


    private void OnEnable()
    {
        TimeManager.instance.TimeStop();
        Button_CloseSelfAllArea.BindOnClickButtonEvent(OnClick_ClosePopup);
        SetSkillSlotOnEnable();
    }

    private void SetSkillSlotOnEnable()
    {
        // 슬롯 정리 - 혹시 오픈 시점에 다른 슬롯들이 있다면 제거하자
        if (_skillSlotList.Count > 0)
        {
            foreach (var slot in _skillSlotList)
            {
                DestroyImmediate(slot.Value.gameObject);
            }
            _skillSlotList.Clear();
        }

        //인벤오픈 1-1) 인벤토리가 열릴때 플레이어가 보유한 모든 아이템을 출력하는 로직을 넣어봅시다
        var skillList = DaniTechGameManager.Inst.GetPlayerSkillList();
        if (skillList == null || skillList.Count == 0)
        {
            Debug.LogWarning("보유한 아이템이 없습니다!");
            return;
        }

        foreach (var skillModel in skillList)
        {
            CreateSlot(skillModel.SkillDataId, skillModel.SkillLevel);
        }
    }


    private void OnDisable()
    {
        // 소멸이니까 나중에 신경써주셔도 되요
        // _itemSlotList.Clear();
        // Destroy
    }

    public void OnClick_ClosePopup()
    {
        DaniTechUIManager.Instance.CloseContentUI(DaniTechUIType.ChooseSkillUI);
        TimeManager.instance.TimeStart();
    }


    public void OnClick_CreateSlotTest()
    {
        // CreateSlot();
    }

    private void CreateSlot(string skillDataId, int skillLevel)
    {
        // 1-1 수동 SetParant가 뒤에 지금은 자동으로 해주고 있다
        var gObj = Instantiate(Prefab_Slot, Transform_UISlotRoot);
        if (gObj == null) return;

        // 1-2 자식 슬롯의 컴포넌트를 가져온다 -> 위에 게임오브젝트는 스크립트가 아직 아니므로
        var slotComponent = gObj.GetComponent<ChooseSkillSlotUI>();
        if (slotComponent == null) return;

        _generatedKey++;

        // 1-3 여기서 slotComponent가지고 뭔가를 하는 겁니다!
        slotComponent.InitSlot(_generatedKey, skillDataId, skillLevel);
        slotComponent.gameObject.name = $"SkillSlot : {slotComponent.SlotInstanceId}";

        // 1-4 중복체크 해주면 좋긴 하지만, 일단 쉽게 컴포넌트(컴포넌트로 게임오브젝트는 받을 수 있으므로)를 보관해보자
        _skillSlotList.Add(slotComponent.SlotInstanceId, slotComponent);

        slotComponent.BindSlotSelectEvent(OnChildSlotSelected);
    }


    private void OnChildSlotSelected(int selectedSlotInstanceId)
    {
        Debug.LogWarning($"자식 슬롯 {selectedSlotInstanceId} 선택됨!");
    }
}
